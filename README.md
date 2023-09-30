## RowsSharp
CSV filtering and editing tool built using C# and WPF
![RowsSharp 23.12 user interface](https://github.com/haruki-taka8/rowsSharp/assets/77907336/2e85de01-4c4f-457f-990a-97763e272fff)
<br><br>

## Features

| Filtering | Editing |
|:----------|:--------|
| Display an image based on the selected row | Add/edit/remove rows and columns |
| Conditional formatting | Add rows with a template |
| Regular expressions | Unlimited undo & redo |
|| Copy/cut/paste |
<br>

## Dependencies
* [material-symbols](https://github.com/marella/material-symbols/)
	* Font converted to `.otf` for WPF compatibility
	* Font content rotated and/or flipped inside the UI
* [XamlBehaviorsWpf](https://github.com/microsoft/XamlBehaviorsWpf)
* [NLog](https://nlog-project.org/)
* [ObservableTable](https://github.com/haruki-taka8/ObservableTable)

A copy of their licenses is available locally at [third-party notices](THIRD-PARTY-NOTICES).
<br>

## Building
Official binaries are built with Visual Studio 2022 and .NET 6 using the _release_ configuration.

<details>
	<summary>More on publish configurations...</summary>

| Item | Configuration |
|------|---------------|
| Configuration | Release, Any CPU |
| Target Framework | net6.0-windows |
| Deployment Mode | Framework-dependent |
| Target Runtime | win-x64 |
| Single File | True |
| ReadyToRun | False |

</details>
