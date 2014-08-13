# HexLight GUI

Language: C# WPF (Windows Presentation Foundation)

An RGB LED controller, supporting advanced colour models.
Compatible with Arduino-based RGB controllers (simple 4-byte serial packet format)
and my custom HexLight hardware project (details not released yet! still working on it..)

The application sits in your Windows system tray, allowing you to rapidly change the LED colour
to suit your mood, whenever you want to:

![PopupScrenshot](https://raw.githubusercontent.com/jorticus/hexlight-gui/master/screenshots/PopupDial.png "PopupScreenshot")

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
- CIE1931 luminance correction! Maps raw RGB values to logarithmic human vision, so they appear properly linear. Note: Gamma is the wrong thing to use for this!
- CIE XYZ/xyY models for device-independant colour control. Work in progress!
- Custom C# WPF colour-picker controls (HSV colour picker, arc-slider, XYZ/xyY colour picker)
- Custom Colour math classes

# TODO

- Implement XYZ/xyY colour picker
- Break protocol interfaces out into separate assemblies, for better extendibility (so people can easily add their own protocols to the GUI)
- RGB+White colour driving model, for more accurate whites
- Send device-independant XYZ values to the controller instead of RGB (RGB doesn't give any guarantees on what colour it will actually produce!)
- Implement XYZ calibration on the controller
- Colour presets
- Blackbody temperature colour (hot/warm/cool)
- Animation (Fade, cycle, strobe)
- Sound reactive lights (as in my project here: http://jared.geek.nz/2013/jan/sound-reactive-led-lights)

# Future ideas

- Interface with f.lux
- Automatically set mood lighting when watching a movie or playing a fullscreen game
