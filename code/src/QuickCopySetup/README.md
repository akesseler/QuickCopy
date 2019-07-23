
## Creating Setups

- First of all rebuild the entire main project at least for each of the _Release_ types (`Release|x86`, `Release|x64`).
  - In doubt check the solution´s _Batch Build_ settings as well as the settings of the _Configuration Manager_  and disable every option for each of the setup projects.
- For the _x86-Setup-MSI_:
  - In the _Visual Studio_ tool-bar select **Release** plus **x86** as output type.
  - Right-click project _QuickCopySetup-x86_ and choose _Rebuild_.
  - The resulting output `qcp-x86.msi` should be found in `bin\x86\Release`.
- For the _x64-Setup-MSI_:
  - In the _Visual Studio_ tool-bar select **Release** plus **x64** as output type.
  - Right-click project _QuickCopySetup-x64_ and choose _Rebuild_.
  - The resulting output `qcp-x64.msi` should be found in `bin\x64\Release`.

Each _Debug_ version should also contain the _Release_ build, because of the _Primary Output_ is set to _Release_.
