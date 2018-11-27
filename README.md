# Taps
C# wrapper and C++ library to use a MOTU 24Ao device with PortAudio

This repository consists of a Microsoft Visual Studio Solution with 3 internal projects:

1. Taps
2. MotuCore
3. SandBox

These projects are meant to communicate C# applications with a MOTU audio interface (model Ao24). The software relies on the usage of the compiled .dll version of the [PortAudio](http://www.portaudio.com/) for x64 machines and a folder named "Phonemes" for playing the 49 haptic signals mapped to 39 English phonemes and 10 extra abbreviations of the most frequent pairs of phonemes in English. The folder must contain 49 .CSV files correctly named after the phoneme naming convention in the project. 

## Taps

The main C# wrapper that uses the compiled .dll of the MotuCore project to communicate with the MOTU interface. More than a wrapper, it represents an interface that makes static external calls to the MotuCore.dll written in C++. Taps offers a singleton design to play phonemes, arbitrary matrices and translate sentences to phonemes using the CMU FLite TTS system (this requires special installation of Flite on the Windows machine using Cygwin).

## MotuCore

Contains the core C++ code that uses PortAudio to send signals to a MOTU 24Ao device. The code is written to export its main funcitonality as a .dll to the Taps C# interface/wrapper.

## Sandbox

A sandbox project written in C# for testing the Taps library and all the implemented functionality. 

