<p align="center">
  <a href="https://github.com/akesseler/QuickCopy/blob/master/LICENSE.md" alt="license">
    <img src="https://img.shields.io/github/license/akesseler/QuickCopy.svg" />
  </a>
  <a href="https://github.com/akesseler/QuickCopy/releases/latest" alt="latest">
    <img src="https://img.shields.io/github/release/akesseler/QuickCopy.svg" />
  </a>
  <a href="https://github.com/akesseler/QuickCopy/archive/master.zip" alt="master">
    <img src="https://img.shields.io/github/languages/code-size/akesseler/QuickCopy.svg" />
  </a>
  <a href="https://github.com/akesseler/QuickCopy/wiki" alt="wiki">
    <img src="https://img.shields.io/badge/wiki-docs-orange.svg" />
  </a>
</p>

## QuickCopy

The _QuickCopy_ program is a command-line oriented Windows console application that 
was inspired by program _FastCopy_. For more information about _FastCopy_ please see 
under [https://fastcopy.jp/en](https://fastcopy.jp/en).

This program is almost fully written in C#. The only exception is the low-level file 
access. For this purpose the functionality of the Win32-API is used. 

### Features 

This program supports UNC file paths which makes it possible to copy files directly 
to a network share without the need of assigning a Windows drive letter.

Furthermore, the program bypasses the `MAX_PATH` limitation of other standard Windows 
file-copy applications. With this in background it easily becomes possible to copy 
files with a path longer than 260 characters.

Another feature is that the program processes all files in parallel. For this purpose 
the C# class `Parallel` is taken. The class itself is configured in a way that one 
processor core is taken per file. Therefore, the maximum number of simultaneously 
processed files depends on the number of actual available processor cores.

### Licensing

The software has been published under the terms of _MIT License_.

### Downloads

The latest release can be obtained from [https://github.com/akesseler/QuickCopy/releases/latest](https://github.com/akesseler/QuickCopy/releases/latest).

The master branch can be downloaded as ZIP from [https://github.com/akesseler/QuickCopy/archive/master](https://github.com/akesseler/QuickCopy/archive/master.zip).

### Documentation

The documentation has been released as Wiki and can be found under [https://github.com/akesseler/QuickCopy/wiki](https://github.com/akesseler/QuickCopy/wiki).

