# DNNpackager

This crude console application will package a DNN module into a Instal zip file.

This is a replacement for MSBuild and is not as far reaching as MSBUILD, but is independant to Visual Studio and simple.

You activate the build by runnig from the windoes prompt or through the VS post compile event.

It requires 1 parameter which is the root folder of the module.

Example:
C:\\DNNpacker.exe C:\Nevoweb\Websites\dev.dnnrocket.com\dev\www\DesktopModules\DNNrocketModules\RocketEcommerce

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
