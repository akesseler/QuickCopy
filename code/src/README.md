
## Project Build

Best way to build the whole project is to use _Visual Studio 2017 Community_. 
Thereafter, download the complete sources, open the solution file `QuickCopy.sln`, 
switch to release, appropriated architecture and rebuild all.

## Program Help

The program help is available through the command-line argument `--help`. See 
below for a list of possible command-line arguments.

```
--source [-s]     The source folder path. This parameter optionally depends
                  on parameter "pattern". Don't use a final backslash if
                  path is enclosed in double-quotes!

--target [-t]     The target folder path. Don't use a final backslash if
                  path is enclosed in double-quotes!

--pattern [-p]    Search pattern, such as "*.*". All files are used by
                  default.

--move [-m]       Move all files instead of copying. Default is OFF.

--verify [-v]     Perform target file verification. Default is OFF.

--overwrite [-o]  Overwrite existing files at target folder. Default is
                  OFF.

--recursive [-r]  Process source folder recursively. Default is OFF.

--console [-c]    Redirect all logging messages onto the console window
                  instead of writing them into the log-file. Default is
                  OFF.

--version         Print program version and other attributes. This argument
                  cannot be used together with other options.

--settings        Print current program settings. This argument cannot be
                  used together with other options.

--help [-?]       Print this little help screen.

<files>           Provide a distinct list of source files. This source file
                  list cannot be used together with parameter "source".
```

## Trouble Shooting

If you get in trouble with the program don’t hesitate to report a bug.

## Creating Setups

The _Visual Studio_ extension _Microsoft Visual Studio Installer Project_ is 
required to be able to create the setup files. Please see the `README.md` file 
in the setup project for more information about creating setup files.
