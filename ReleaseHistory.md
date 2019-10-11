### Version 0.7.10, 2019/10/11:

* new option to select .NET Core version, use latest installed by default. ([#21](https://github.com/szehetner/InliningAnalyzer/issues/21))
* bugfix for methods with pointer parameters

### Version 0.7.9, 2019/04/21:

* bugfix for methods with "in" parameters ([#19](https://github.com/szehetner/InliningAnalyzer/issues/19))

### Version 0.7.8, 2019/03/29:

* fixed exception when analyzing methods with generic parameters ([#18](https://github.com/szehetner/InliningAnalyzer/issues/18))

### Version 0.7.7, 2019/03/13:

* support for operator overloads (highlighting calls to and inside of overloaded operators) ([#17](https://github.com/szehetner/InliningAnalyzer/issues/17))
* support for .NET Core 3.0

### Version 0.7.6, 2019/03/11:

* support for Visual Studio 2019 ([#15](https://github.com/szehetner/InliningAnalyzer/issues/15))

### Version 0.7.5, 2019/01/05:

* support for dark color theme ([#12](https://github.com/szehetner/InliningAnalyzer/issues/12))
* fix for missing assembly errors in Web Application Projects ([#13](https://github.com/szehetner/InliningAnalyzer/issues/13))
* async extension package loading
* increase responsiveness by running analyzer and publishing (if applicable) on background thread

### Version 0.7.4, 2018/11/16:

* bugfix for resolving dependencies of .Net Standard and .NET Core projects ([#11](https://github.com/szehetner/InliningAnalyzer/issues/11))
* bugfix for methods with ref parameters ([#10](https://github.com/szehetner/InliningAnalyzer/issues/10))
* support for manually selecting the assembly to analyze

### Version 0.7.3, 2018/10/31:

* bugfix for missing assembly error with .NET Core projects ([#8](https://github.com/szehetner/InliningAnalyzer/issues/8))
* bugfix for ref return methods  ([#9](https://github.com/szehetner/InliningAnalyzer/issues/9))

### Version 0.7.2, 2018/08/25:

* fixed various bugs causing the Analyzer to not work due to wrong dependencies (both for .NET Core and .NET Framework)

### Version 0.7.1, 2018/07/11:

* bugfix for missing highlighting
* changed context menu action to "Run Inlining Analyzer on Current Scope". If invoked within a method, it analyzes only this method, if invoked anywhere else within a class/struct, it analyzes all methods of this class/struct.

### Version 0.7.0, 2018/05/07:

* support for .Net Core (2.1+ must be installed to run the analyzer, the projects being analyzed can have any .Net Core or .Net Standard version)
* options dialog for setting a preferred runtime (.Net Framework or .Net Core) to be used for analyzing .NET Standard projects or projects targeting multiple frameworks)

### Version 0.6.5, 2018/04/20:

* support for methods with out parameters
* various bugfixes

### Version 0.6.4, 2018/03/09:

* highlight calls in async methods
* support for ref return properties ([#3](https://github.com/szehetner/InliningAnalyzer/issues/3))

### Version 0.6.3, 2017/11/27

* Fixed false "not inlined" results ([#2](https://github.com/szehetner/InliningAnalyzer/issues/2)) by analyzing the calltree first and compiling methods in the right order (callers before callees)
* highlight calls in iterator methods (i.e. methods containing yield return)

### Version 0.6.0, 2017/10/08
* Initial Release
