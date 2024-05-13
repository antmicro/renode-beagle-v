# Renode simulation of the BeagleV platform

Copyright (c) 2021-2024 [Antmicro](https://www.antmicro.com/)

[![View on Antmicro Open Source Portal](https://img.shields.io/badge/View%20on-Antmicro%20Open%20Source%20Portal-332d37?style=flat-square)](https://opensource.antmicro.com/projects/renode-beagle-v) 

This repository contains instructions how to simulate OpenSBI, U-Boot and Linux on [the BeagleV platform](https://beaglev.seeed.cc) in [Renode](https://renode.io).

## Installing Renode

To run this platform it's recommended to install the latest Renode (not earlier than [`b339f650a54dbbbe26cd3a1eeb802277cfdc2982`](https://github.com/renode/renode/commit/b339f650a54dbbbe26cd3a1eeb802277cfdc2982) is required).
See the [documentation](https://docs.renode.io/en/latest/introduction/installing.html) for details.

For Linux, you can use the portable version:
```
wget https://builds.renode.io/renode-latest.linux-portable.tar.gz 
mkdir renode_portable
tar xf renode-latest.linux-portable.tar.gz -C renode_portable --strip-components=1
export PATH="`pwd`/renode_portable:$PATH"
```

To run Robot tests, you will also need to install some Python dependencies:
```
python3 -m pip install -r renode_portable/tests/requirements.txt
```

## Running the simulation

Assuming that Renode is installed and available in PATH, to run the demo execute:

    renode -e "start @scripts/single-node/beaglev_starlight.resc"

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
U-Boot 2021.01-g7dac1a6e-dirty (Apr 26 2021 - 09:57:51 +0000)
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
Hit any key to stop autoboot:  0
## Loading kernel from FIT Image at a0000000 ...
   Using 'config-1' configuration
   Trying 'vmlinux' kernel subimage
     Description:  vmlinux
     Type:         Kernel Image
     Compression:  uncompressed
     Data Start:   0xa00000c8
     Data Size:    17660416 Bytes = 16.8 MiB
     Architecture: RISC-V
     OS:           Linux
     Load Address: 0x80200000
     Entry Point:  0x80200000
   Verifying Hash Integrity ... OK
## Loading fdt from FIT Image at a0000000 ...
   Using 'config-1' configuration
   Trying 'fdt' fdt subimage
     Description:  unavailable
     Type:         Flat Device Tree
     Compression:  uncompressed
     Data Start:   0xa4f297f8
     Data Size:    16655 Bytes = 16.3 KiB
     Architecture: RISC-V
     Load Address: 0x86000000
   Verifying Hash Integrity ... OK
   Loading fdt from 0xa4f297f8 to 0x86000000
   Booting using the fdt blob at 0x86000000
## Loading loadables from FIT Image at a0000000 ...
   Trying 'ramdisk' loadables subimage
     Description:  buildroot initramfs
     Type:         RAMDisk Image
     Compression:  uncompressed
     Data Start:   0xa10d7b7c
     Data Size:    65346560 Bytes = 62.3 MiB
     Architecture: RISC-V
     OS:           Linux
     Load Address: 0x86100000
     Entry Point:  unavailable
   Verifying Hash Integrity ... OK
   Loading loadables from 0xa10d7b7c to 0x86100000
   Loading Kernel Image
## Flattened Device Tree blob at 86000000
   Booting using the fdt blob at 0x86000000
   Using Device Tree in place at 0000000086000000, end 000000008600710e
Starting kernel ...
[    0.000000] Linux version 5.10.6-g93604a0a3ecc (root@runner-7005a2b9-project-347
5-concurrent-0) (riscv64-buildroot-linux-gnu-gcc.br_real (Buildroot -g2e13b6d) 10.2
.0, GNU ld (GNU Binutils) 2.34) #1 SMP Mon Apr 26 09:57:54 UTC 2021
[    0.000000] OF: fdt: Ignoring memory range 0x80000000 - 0x80200000
[    0.000000] earlycon: sbi0 at I/O port 0x0 (options '')
[    0.000000] printk: bootconsole [sbi0] enabled
...
```

To log in to the system, use the following credentials:

```
user: root
password: starfive
```

## Automatic testing of the simulation

You can also run an automated [robot](https://robotframework.org/) test:

    renode-test renode_portable/tests/platforms/BeagleV-StarLight.robot
   
You should see the following result:
   
    Preparing suites
    Started Renode instance on port 49152; pid 1756415
    Starting suites
    Running renode_portable/tests/platforms/BeagleV-StarLight.robot
    +++++ Starting test 'BeagleV-StarLight.Should Boot U-Boot'
    +++++ Finished test 'BeagleV-StarLight.Should Boot U-Boot' in 6.16 seconds with status OK
    +++++ Starting test 'BeagleV-StarLight.Should Provide Two Linux Machines With Ethernet Connection'
    +++++ Finished test 'BeagleV-StarLight.Should Provide Two Linux Machines With Ethernet Connection' in 88.38 seconds with status OK
    +++++ Starting test 'BeagleV-StarLight.Should Ping'
    +++++ Finished test 'BeagleV-StarLight.Should Ping' in 88.22 seconds with status OK
    +++++ Starting test 'BeagleV-StarLight.Should Ping Large Payload'
    +++++ Finished test 'BeagleV-StarLight.Should Ping Large Payload' in 91.71 seconds with status OK
    Suite renode_portable/tests/platforms/BeagleV-StarLight.robot finished successfully in 274.77 seconds.
    Cleaning up suites
    Closing Renode pid 1756415
    Aggregating all robot results
    Output:  /home/user/robot_output.xml
    Log:     /home/user/log.html
    Report:  /home/user/report.html
    Tests finished successfully :)
