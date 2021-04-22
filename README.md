# Renode simulation of the BeagleV platform

Copyright (c) 2021 Antmicro ([www.antmicro.com](https://www.antmicro.com/))

This repository contains a script, platform definition, test suite and custom peripheral models necessary to simulate OpenSBI, U-Boot and Linux on [the BeagleV platform](https://beaglev.seeed.cc) in [Renode](https://renode.io).

## Installing Renode

To run this platform you need Renode 1.12 installed in the host operating system (see the [documentation](https://docs.renode.io/en/latest/introduction/installing.html) for details).

For Linux, you can use the portable version:
```
wget https://github.com/renode/renode/releases/download/v1.12.0/renode-1.12.0.linux-portable.tar.gz
mkdir renode_portable
tar xf renode-1.12.0.linux-portable.tar.gz -C renode_portable --strip-components=1
cd renode_portable
export PATH="`pwd`:$PATH"
```

To run Robot tests, you will also need to install some Python dependencies:
```
python3 -m pip install -r tests/requirements.txt
```

## Running the simulation

Assuming that Renode is installed and available in PATH, to run the demo check out this repository, enter it and execute:

    renode beaglev.resc

You should see the following output on UART:

```
OpenSBI v0.9
   ____                    _____ ____ _____
  / __ \                  / ____|  _ \_   _|
 | |  | |_ __   ___ _ __ | (___ | |_) || |
 | |  | | '_ \ / _ \ '_ \ \___ \|  _ < | |
 | |__| | |_) |  __/ | | |____) | |_) || |_
  \____/| .__/ \___|_| |_|_____/|____/_____|
        | |
        |_|

Platform Name             : StarFive VIC7100
Platform Features         : timer,mfdeleg
Platform HART Count       : 2
Firmware Base             : 0x80000000
Firmware Size             : 92 KB
Runtime SBI Version       : 0.2

Domain0 Name              : root
Domain0 Boot HART         : 1
Domain0 HARTs             : 0*,1*
Domain0 Region00          : 0x0000000080000000-0x000000008001ffff ()
Domain0 Region01          : 0x0000000000000000-0xffffffffffffffff (R,W,X)
Domain0 Next Address      : 0x0000000080020000
Domain0 Next Arg1         : 0x0000000088000000
Domain0 Next Mode         : S-mode
Domain0 SysReset          : yes

Boot HART ID              : 1
Boot HART Domain          : root
Boot HART ISA             : rv64imafdcs
Boot HART Features        : scounteren,mcounteren,time
Boot HART PMP Count       : 16
Boot HART PMP Granularity : 4
Boot HART PMP Address Bits: 54
Boot HART MHPM Count      : 0
Boot HART MHPM Count      : 0
Boot HART MIDELEG         : 0x0000000000000222
Boot HART MEDELEG         : 0x000000000000b109


U-Boot 2021.01-g7dac1a6e-dirty (Apr 21 2021 - 22:21:51 +0000)

ofnode_read_prop: riscv,isa: rv64imafdc
CPU:   rv64imafdc
Model: sifive,freedom-u74-arty
DRAM:  8 GiB
ofnode_read_prop: tick-timer: <not found>
ofnode_read_u32_array: ranges: ofnode_read_u32_array: ranges: ofnode_read_u32_index
: timebase-frequency: (not found)
ofnode_read_u32_index: timebase-frequency: 0x5f5e10 (6250000)
ofnode_read_u32_index: timebase-frequency: (not found)
ofnode_read_u32_index: timebase-frequency: 0x5f5e10 (6250000)
MMC:   VIC DWMMC0: 0
In:    serial
Out:   serial
Err:   serial
Model: sifive,freedom-u74-arty
Net:   ofnode_read_prop: tick-timer: <not found>
dwmac.10020000
StarFive # version
U-Boot 2021.01-g7dac1a6e-dirty (Apr 21 2021 - 22:21:51 +0000)

riscv64-buildroot-linux-gnu-gcc.br_real (Buildroot -g2e13b6d) 10.2.0
GNU ld (GNU Binutils) 2.34
StarFive #
```

## Booting Linux

To boot Linux, you must provide U-Boot with the following command:

```
StarFive # setenv fileaddr a0000000;bootm start ${fileaddr};bootm loados ${fileaddr};booti 0x80200000 0x86100000:${filesize} 0x86000000
```

This will load the kernel and the rootfs from RAM.

To log in to the system, use the following credentials:

```
user: root
password: starfive
```

## Automatic testing of the simulation

You can also run an automated [robot](https://robotframework.org/) test:

    renode-test beaglev.robot
   
You should see the following result:
    
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
    
