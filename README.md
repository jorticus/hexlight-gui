# HexLight GUI

Language: C# WPF (Windows Presentation Foundation)

An RGB LED controller, supporting advanced colour models.
Compatible with Arduino-based RGB controllers (simple 4-byte serial packet format)
and my custom HexLight hardware project (details not released yet! still working on it..)

The application sits in your Windows system tray, allowing you to rapidly change the LED colour
to suit your mood, whenever you want to:

![PopupScrenshot](https://raw.githubusercontent.com/jorticus/hexlight-gui/master/screenshots/PopupDial2.png "PopupScreenshot")

**THIS PROJECT IS A WORK IN PROGRESS**

But feel free to use it in your own projects!

It probably won't work out-of-the-box for you. So read the source code to figure out how to interface
it to your hardware of choice. I've made the hardware control fairly abstract, so it should be
simple to add your own controllers

Also experimenting with advanced colour models such as CIE XYZ, CIE xyY, Blackbody Temperature, etc.
Do NOT assume my advanced colour model code works correctly! It doesn't, and I'm working on it.

If you spot any math errors, let me know!

# Features

- Easy to interface to arduinos through serial or a TCP socket (eg. TCP to serial bridge on a server).
- Support for a custom 4-channel RGB controller based on a PIC32, through serial or RS485.
- CIE1931 luminance correction! Maps linear RGB values to logarithmic human vision. Note: Gamma is the wrong thing to use for this!
- CIE XYZ/xyY models for device-independant colour control. Work in progress!
- Custom C# WPF colour-picker controls

