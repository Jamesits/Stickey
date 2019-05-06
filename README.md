# Stickey

Simple [Raw Input](https://docs.microsoft.com/en-us/windows/desktop/inputdev/about-raw-input) application which converts joystick events to keyboard events.

## Mapping

Currently we don't have any UI to edit key mapping. This mapping is designed for [The Binding of Issac](https://store.steampowered.com/app/113200/The_Binding_of_Isaac/).

 * left 4-way pad: WASD
 * ABXY: keyboard Up/Down/Left/Right
 * LB: E
 * RB: Space
 * View: Q

## Supported hardware

* Xbox One controller
* Xbox 360 controller

## Compile

Use VS2017 or later.

## Usage

Run the program and click "Start".

## Known issues

* No UI to edit mapping
* You need to explicitly switch off IME (this is a game issue, not our issue)

## Thanks

This project is a derived work of [Using Raw Input from C# to handle multiple keyboards](https://www.codeproject.com/Articles/17123/Using-Raw-Input-from-C-to-handle-multiple-keyboard).

Thank the following people for their generous help and support:

* [David Huang](https://github.com/hjc4869)
* [Ikeltis](https://github.com/ikeltis)
