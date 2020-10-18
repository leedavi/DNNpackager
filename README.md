# DNNpackager

This crude console application will package a DNN module into a Instal zip file.

This is a replacement for MSBuild and is not as far reaching as MSBUILD, but is independant to Visual Studio and simple.

You activate the build by runnig from the windoes prompt or through the VS post compile event.

It requires 1 parameter which is the root folder of the module.

Example:
C:\\DNNpackager.exe C:\Websites\www\DesktopModules\DNNtestModules\testproject

It should also have a configuration file in the module root folder called "DNNpackager.config".

Example:
```xml
<root>
	<version>1.0.0</version>
	<!-- Include only files that match the regular expression -->
	<regexpr>(\.cshtml|\.html|\.resx|\.dnn|\.png|\.css|\.js|\.xml|\.txt|\.md)$</regexpr>
	<directory include='false'>
		<!-- All paths should be from the source root (project root) -->
		<value>\.git</value>
		<value>\.vs</value>
		<value>\bin</value>
		<value>\Componants</value>
		<value>\Documentation</value>
		<value>\Examples</value>
		<value>\Interfaces</value>
		<value>\obj</value>
		<value>\packages</value>
		<value>\Providers</value>
		<value>\Theme</value>
	</directory>
	<file include='false'>
		<!-- All paths should be from the source root (project root) -->
		<value></value>
	</file>
	<!-- The bin path gives the backward folder levels to the DNN bin folder, so assemblies can be taken. -->
	<!-- In some cases, if the project folder is not under a company level, this will need to be change. -->
	<binfolder>\\..\\..\\..\\bin</binfolder>
	<assembly>
		<!-- Assembllies will be taken from the DNN bin folder, and placed on root. -->
		<value>test.dll</value>
	</assembly>
</root>
```

Installation
------------

There is currently no installation package.  But if you're running Visual Studio with DNN, the chances are you already have any dependancies.

So simply download the release exe or the source code form GITHUB and setup in a suitable directory.

Setup in Visual Studio
----------------------

You can setup DNNpacker in Visual Studio, so it can be started easily

In the Visual Studio menu.
1. "Tools>External Tool"
2. "Add" with the name "DNNpackager"
3. Enter the correct path to DNNpackager.exe. (Where you placed it during installation)
4. Check "Prompt for Arguments"

When you run DNNpackager from the Visual Studio menu.  "Tools>DNNpackager" You will be prompted for aguments, select the "Project Directory - $(ProjectDir)"

Click "OK" and the DNNpackger will run.  It should create an install zip file the $(ProjectDir)\Installation folder.

