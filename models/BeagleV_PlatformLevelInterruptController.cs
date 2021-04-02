//
// Copyright (c) 2010-2021 Antmicro
//
// This file is licensed under the MIT License.
// Full license text is available in 'licenses/MIT.txt'.
//
using System;
using System.Collections.Generic;
using System.Linq;
using Antmicro.Renode.Core;
using Antmicro.Renode.Core.Structure.Registers;
using Antmicro.Renode.Exceptions;
using Antmicro.Renode.Logging;
using Antmicro.Renode.Peripherals.Bus;
using Antmicro.Renode.Utilities;
using Antmicro.Renode.Peripherals.CPU;
using Antmicro.Renode.Peripherals.IRQControllers.PLIC;

namespace Antmicro.Renode.Peripherals.IRQControllers
{
    [AllowedTranslations(AllowedTranslation.ByteToDoubleWord)]
    public class BeagleV_PlatformLevelInterruptController : IPlatformLevelInterruptController, IDoubleWordPeripheral, IIRQController, INumberedGPIOOutput, IKnownSize
    {
        public BeagleV_PlatformLevelInterruptController(int numberOfSources, int numberOfContexts = 1, bool prioritiesEnabled = true)
        {
            if(numberOfSources + 1 > MaxSources)
            {
                throw new ConstructionException($"Current {this.GetType().Name} implementation does not support more than {MaxSources} sources");
            }

            var connections = new Dictionary<int, IGPIO>();
            for(var i = 0; i < numberOfContexts; i++)
            {
                connections[i] = new GPIO();
            }
            Connections = connections;

            irqSources = new IrqSource[numberOfSources];
            for(var i = 0u; i < numberOfSources; i++)
            {
                irqSources[i] = new IrqSource(i, this);
            }

            irqContexts = new IrqContext[numberOfContexts];
            for(var i = 0u; i < irqContexts.Length; i++)
            {
                irqContexts[i] = new IrqContext(i, this);
            }

            var registersMap = new Dictionary<long, DoubleWordRegister>();

            registersMap.Add((long)Registers.Source0Priority, new DoubleWordRegister(this)
                .WithValueField(0, 3, FieldMode.Read, writeCallback: (_, value) =>
                {
                    if(value != 0)
                    {
                        this.Log(LogLevel.Warning, $"Trying to set priority {value} for Source 0, which is illegal");
                    }
                }));
            for(var i = 1; i < numberOfSources; i++)
            {
                var j = i;
                registersMap[(long)Registers.Source1Priority * i] = new DoubleWordRegister(this)
                    .WithValueField(0, 3,
                                    valueProviderCallback: (_) => irqSources[j].Priority,
                                    writeCallback: (_, value) =>
                                    {
                                        if(prioritiesEnabled)
                                        {
                                            irqSources[j].Priority = value;
                                            RefreshInterrupts();
                                        }
                                    });
            }

            for(var i = 0u; i < numberOfContexts; i++)
            {
                AddContextEnablesRegister(registersMap, (long)Registers.Context0Enables + i * ContextEnablesWidth, i, numberOfSources);
                AddContextClaimCompleteRegister(registersMap, (long)Registers.Context0ClaimComplete + i * ContextClaimWidth, i);
            }

            registers = new DoubleWordRegisterCollection(this, registersMap);
        }

        public uint ReadDoubleWord(long offset)
        {
            return registers.Read(offset);
        }

        public void Reset()
        {
            this.Log(LogLevel.Noisy, "Resetting peripheral state");

            registers.Reset();
            foreach(var irqSource in irqSources)
            {
                irqSource.Reset();
            }
            foreach(var irqContext in irqContexts)
            {
                irqContext.Reset();
            }
            RefreshInterrupts();
        }

        public void WriteDoubleWord(long offset, uint value)
        {
            registers.Write(offset, value);
        }

        public void OnGPIO(int number, bool value)
        {
            if(!IsIrqSourceAvailable(number))
            {
                this.Log(LogLevel.Error, "Wrong gpio source: {0}", number);
                return;
            }
            lock(irqSources)
            {
                this.Log(LogLevel.Noisy, "Setting GPIO number #{0} to value {1}", number, value);
                var irq = irqSources[number];
                irq.State = value;
                irq.IsPending |= value;
                RefreshInterrupts();
            }
        }

        public IReadOnlyDictionary<int, IGPIO> Connections { get; }

        public int ForcedTarget
        { 
            get => -1;
            set
            {
                throw new RecoverableException("Forcing targets not supported");
            }
        }

        public long Size => 0x4000000;

        private bool IsIrqSourceAvailable(int number)
        {
            return number >= 0 && number < irqSources.Length;
        }

        private void RefreshInterrupts()
        {
            lock(irqSources)
            {
                foreach(var context in irqContexts)
                {
                    context.RefreshInterrupt();
                }
            }
        }

        private void AddContextClaimCompleteRegister(Dictionary<long, DoubleWordRegister> registersMap, long offset, uint contextId)
        {
            registersMap.Add(offset, new DoubleWordRegister(this).WithValueField(0, 32, valueProviderCallback: _ =>
            {
                lock(irqSources)
                {
                    return irqContexts[contextId].AcknowledgePendingInterrupt();
                }
            },
            writeCallback: (_, value) =>
            {
                if(!IsIrqSourceAvailable((int)value))
                {
                    this.Log(LogLevel.Error, "Trying to complete handling of non-existing interrupt source {0}", value);
                    return;
                }
                lock(irqSources)
                {
                    irqContexts[contextId].CompleteHandlingInterrupt(irqSources[value]);
                }
            }));
        }

        private void AddContextEnablesRegister(Dictionary<long, DoubleWordRegister> registersMap, long address, uint contextId, int numberOfSources)
        {
            var maximumSourceDoubleWords = (int)Math.Ceiling((numberOfSources + 1) / 32.0) * 4;

            for(var offset = 0u; offset < maximumSourceDoubleWords; offset += 4)
            {
                var lOffset = offset;
                registersMap.Add(address + offset, new DoubleWordRegister(this).WithValueField(0, 32, writeCallback: (_, value) =>
                {
                    lock(irqSources)
                    {
                        // Each source is represented by one bit. offset and lOffset indicate the offset in double words from ContextXEnables,
                        // `bit` is the bit number in the given double word,
                        // and `sourceIdBase + bit` indicate the source number.
                        var sourceIdBase = lOffset * 8;
                        var bits = BitHelper.GetBits(value);
                        for(var bit = 0u; bit < bits.Length; bit++)
                        {
                            var sourceNumber = sourceIdBase + bit;
                            if(!IsIrqSourceAvailable((int)sourceNumber))
                            {
                                if(bits[bit])
                                {
                                    this.Log(LogLevel.Warning, "Trying to enable non-existing source: {0}", sourceNumber);
                                }
                                continue;
                            }

                            irqContexts[contextId].EnableSource(irqSources[sourceNumber], bits[bit]);
                        }
                        RefreshInterrupts();
                    }
                }));
            }
        }

        private DoubleWordRegisterCollection registers;

        private readonly IrqSource[] irqSources;
        private readonly IrqContext[] irqContexts;

        private const long ContextEnablesWidth = Registers.Context1Enables - Registers.Context0Enables;
        private const long ContextClaimWidth = Registers.Context1ClaimComplete - Registers.Context0ClaimComplete;
        private const uint MaxSources = 1024;

        private enum Registers : long
        {
            Source0Priority = 0x0, //this is a fake register, as there is no source 0, but the software writes to it anyway.
            Source1Priority = 0x4,
            Source2Priority = 0x8,
            // ...
            StartOfPendingArray = 0x1000,
            Context0Enables = 0x2000,
            Context1Enables = 0x2080,
            Context2Enables = 0x2100,
            // ...
            Context0PriorityThreshold = 0x200000,
            Context0ClaimComplete = 0x200004,
            // ...
            Context1PriorityThreshold = 0x201000,
            Context1ClaimComplete = 0x201004,
            // ...
            Context2PriorityThreshold = 0x202000,
            Context2ClaimComplete = 0x202004,
            //
            Context3PriorityThreshold = 0x203000,
            Context3ClaimComplete = 0x203004,
            //
            Context4PriorityThreshold = 0x204000,
            Context4ClaimComplete = 0x204004,
            // ...
        }

        private class IrqContext
        {
            public IrqContext(uint id, IPlatformLevelInterruptController irqController)
            {
                this.irqController = irqController;
                this.id = id;

                enabledSources = new HashSet<IrqSource>();
                activeInterrupts = new Stack<IrqSource>();
            }

            public override string ToString()
            {
                return $"[Context #{id}]";
            }

            public void Reset()
            {
                activeInterrupts.Clear();
                enabledSources.Clear();

                RefreshInterrupt();
            }

            public void RefreshInterrupt()
            {
                var forcedTarget = irqController.ForcedTarget;
                if(forcedTarget != -1 && this.id != forcedTarget)
                {
                    irqController.Connections[(int)this.id].Set(false);
                    return;
                }

                var currentPriority = activeInterrupts.Count > 0 ? activeInterrupts.Peek().Priority : 0;
                var isPending = enabledSources.Any(x => x.Priority > currentPriority && x.IsPending);
                irqController.Connections[(int)this.id].Set(isPending);
            }

            public void CompleteHandlingInterrupt(IrqSource irq)
            {
                irqController.Log(LogLevel.Noisy, "Completing irq {0} at {1}", irq.Id, this);

                if(activeInterrupts.Count == 0)
                {
                    irqController.Log(LogLevel.Error, "Trying to complete irq {0} @ {1}, there are no active interrupts left", irq.Id, this);
                    return;
                }
                var topActiveInterrupt = activeInterrupts.Pop();
                if(topActiveInterrupt != irq)
                {
                    irqController.Log(LogLevel.Error, "Trying to complete irq {0} @ {1}, but {2} is the active one", irq.Id, this, topActiveInterrupt.Id);
                    return;
                }

                irq.IsPending = irq.State;
                RefreshInterrupt();
            }

            public void EnableSource(IrqSource s, bool enabled)
            {
                if(enabled)
                {
                    enabledSources.Add(s);
                }
                else
                {
                    enabledSources.Remove(s);
                }
                irqController.Log(LogLevel.Noisy, "{0} source #{1} @ {2}", enabled ? "Enabling" : "Disabling", s.Id, this);
                RefreshInterrupt();
            }

            public uint AcknowledgePendingInterrupt()
            {
                IrqSource pendingIrq;

                var forcedTarget = irqController.ForcedTarget;
                if(forcedTarget != -1 && this.id != forcedTarget)
                {
                    pendingIrq = null;
                }
                else
                {
                    pendingIrq = enabledSources.Where(x => x.IsPending)
                        .OrderByDescending(x => x.Priority)
                        .ThenBy(x => x.Id).FirstOrDefault();
                }

                if(pendingIrq == null)
                {
                    irqController.Log(LogLevel.Noisy, "There is no pending interrupt to acknowledge at the moment for {0}. Currently enabled sources: {1}", this, string.Join(", ", enabledSources.Select(x => x.ToString())));
                    return 0;
                }
                pendingIrq.IsPending = false;
                activeInterrupts.Push(pendingIrq);

                irqController.Log(LogLevel.Noisy, "Acknowledging pending interrupt #{0} @ {1}", pendingIrq.Id, this);

                RefreshInterrupt();
                return pendingIrq.Id;
            }

            private readonly uint id;
            private readonly IPlatformLevelInterruptController irqController;
            private readonly HashSet<IrqSource> enabledSources;
            private readonly Stack<IrqSource> activeInterrupts;
        }
    }
}
