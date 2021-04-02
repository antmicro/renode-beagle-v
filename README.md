# Renode's simulation of the BeagleV platform
Copyright (c) 2021 Antmicro ([www.antmicro.com](https://www.antmicro.com/))

This repository contains script, platform defitinion, test suite and custom peripherals models necessary to simulate OpenSBI and Uboot on [the BeagleV platform](https://beaglev.seeed.cc) in Renode.

To run those it is required to have Renode in version at least 1.12 installed in the host operating system (see the [documentation](https://docs.renode.io/en/latest/introduction/installing.html) for details).

## Running the simulation

Assuming that Renode is installed and available in PATH, to run the demo execute:

    renode beagelv.resc

You should see the following output on UART:

```
Device Tree can't be expanded to accmodate new node
OpenSBI v0.6
   ____                    _____ ____ _____
  / __ \                  / ____|  _ \_   _|
 | |  | |_ __   ___ _ __ | (___ | |_) || |
 | |  | | '_ \ / _ \ '_ \ \___ \|  _ < | |
 | |__| | |_) |  __/ | | |____) | |_) || |_
  \____/| .__/ \___|_| |_|_____/|____/_____|
        | |
        |_|
Platform Name          : BeagleV
Platform HART Features : RV64ACDFIMS
Platform Max HARTs     : 2
Current Hart           : 0
Firmware Base          : 0x80000000
Firmware Size          : 76 KB
Runtime SBI Version    : 0.2
MIDELEG : 0x0000000000000222
MEDELEG : 0x000000000000b109
PMP0    : 0x0000000080000000-0x000000008001ffff (A)
PMP1    : 0x0000000000000000-0xffffffffffffffff (A,R,W,X)
U-Boot 2021.04-rc4-g6da3416bfa (Apr 02 2021 - 10:49:10 +0000)
CPU:   rv64imafdc
Model: BeagleV
DRAM:  8 GiB
In:    serial@11870000
Out:   serial@11870000
Err:   serial@11870000
Model: BeagleV
Net:   
Warning: gmac@10020000 (eth0) using random MAC address - e2:43:70:da:ad:1b
eth0: gmac@10020000
Hit any key to stop autoboot:  0 
=> version
U-Boot 2021.04-rc4-g6da3416bfa (Apr 02 2021 - 10:49:10 +0000)
riscv64-linux-gnu-gcc (Debian 8.3.0-2) 8.3.0
GNU ld (GNU Binutils for Debian) 2.31.1
=> 
```

## Automatic testing of the simulation

You can also run an automated robot test:

    renode-test beagelv.robot
   
As a result you should see the following result:
    
    Preparing suites
    Started Renode instance on port 9999; pid 3238544
    Starting suites
    Running /home/user/renode-beagle-v/beaglev.robot
    +++++ Starting test 'beaglev.Should Print Help'
    +++++ Finished test 'beaglev.Should Print Help' in 4.16 seconds with status OK
    Cleaning up suites
    Closing Renode pid 3238544
    Aggregating all robot results
    Output:  /home/user/renode-beagle-v/robot_output.xml
    Log:     /home/user/renode-beagle-v/log.html
    Report:  /home/user/renode-beagle-v/report.html
    Tests finished successfully :)
    
