# CHROMA-8
A CHIP-8 interpreter based on the Chroma engine.

![GitHub code size in bytes](https://img.shields.io/github/languages/code-size/Hacktix/CHROMA-8)
![GitHub Release Date](https://img.shields.io/github/release-date/Hacktix/CHROMA-8)

## What is CHIP-8?
CHIP-8 is an interpreted programming language, developed by Joseph Weisbecker. It was initially used on the COSMAC VIP and Telmac 1800 8-bit microcomputers in the mid-1970s. CHIP-8 programs are run on a CHIP-8 virtual machine. It was made to allow video games to be more easily programmed for these computers. [(Source)](https://en.wikipedia.org/wiki/CHIP-8)

## The Emulator
This is the third CHIP-8 emulator I have developed, after a first attempt in C# using OpenGL libraries and a second (more optimized) attempt using SDL2 with C++. However, out of all three, the Chroma-based version is my personal favorite, mostly due to the simplicity of the engine and impressive results.

Video output is handled by the Chroma engine entirely, Audio output uses my [ChromaSynth](https://github.com/Hacktix/ChromaSynth) library for at-runtime audio synthesis.

## Included ROMs
All ROMs provided in this repository are sourced from [this repository](https://github.com/dmatlack/chip8/tree/master/roms), which is a collection of Public Domain ROMs, as well as [this](https://github.com/corax89/chip8-test-rom) and [this](https://slack-files.com/T3CH37TNX-F3RF5KT43-0fb93dbd1f) test ROM, the latter of which is documented [here](https://slack-files.com/T3CH37TNX-F3RKEUKL4-b05ab4930d).

## Screenshots
![Pong](https://i.imgur.com/DLtf50f.png)
![Space Invaders](https://i.imgur.com/OI8hpFa.png)