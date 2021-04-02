*** Settings ***
Suite Setup                   Setup
Suite Teardown                Teardown
Test Setup                    Reset Emulation
Test Teardown                 Test Teardown
Resource                      ${RENODEKEYWORDS}

*** Test Cases ***
Should Print Help
    Execute Command          include @${CURDIR}/beaglev.resc
    Create Terminal Tester   sysbus.uart0

    Start Emulation

    Wait For Line On Uart    OpenSBI v0.6
    Wait For Line On Uart    Platform Name\\s+: BeagleV     treatAsRegex=true

    Wait For Line On Uart    U-Boot 2021.04
    Wait For Line On Uart    Model: BeagleV

    Wait For Prompt On Uart  Hit any key to stop autoboot
    # send Q press
    Send Key To Uart         0x51

    Wait For Prompt On Uart  =>
    Write Line To Uart       help

    Wait For Line On Uart    base\\s+ - print or set address offset   treatAsRegex=true
    Wait For Line On Uart    cp\\s+ - memory copy                     treatAsRegex=true
    Wait For Line On Uart    unzip\\s+ - unzip a memory region        treatAsRegex=true

