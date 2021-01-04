# DNNpackager

This console application is designed to allow the source of a module to be edited when not part of the webite structure.
It will copy the installation files to the website, as if the module has been installed.  

It will also package a DNN module into an Install zip file.

It should also have a configuration file in the Project root folder called "DNNpackager.dnnpack".

Example:
```xml
<root>
	<version>1.0.0</version>
	<!-- Name of zip file, optional.  Directory name is used if left blank -->
	<name></name>
	<!-- Include only files that match the regular expression -->
	<regexpr>(\.cshtml|\.html|\.resx|\.dnn|\.png|\.css|\.js|\.xml|\.txt|\.md)$</regexpr>
	<directory include='false'>
		<!-- Folders that will NOT be placed into the installation zip -->
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
	<file include='true'>
		<!-- Extra files that need to be inclued. -->
		<!-- All paths should be from the source root (project root) -->
		<value></value>
	</file>
	<assembly>
		<!-- Assembllies will be taken from the DNN bin folder, and placed on root. -->
		<value>test.dll</value>
	</assembly>
</root>
```

Installation
------------

Use the Installation Package to install on the local machine.

Create a BAT file
-----------------

The easiest way to run DNNpackager is to create a BAT file that executes the assembly whne VS compiles the code.
 
The DNNpackager take 3 arguments.  

DNNpackager.exe \<ProjectDir\> \<Website Module Folder\> \<Website bin Folder\>

**ProjectDir** =  The root folder of the source project.

**Website Module Folder** = The module folder in the website where the module files will be copied to.

**Website bin Folder** = The bin folder of the website, where the assembly needs to be copied.

Example:
```

"C:\Program Files (x86)\Nevoweb\DNNpackager\DNNpackager.exe"  "$(ProjectDir)" "C:\Nevoweb\Temp\copytest" "$(ProjectDir)$(OutDir)" "C:\Nevoweb\Temp\bin"

```

Copy to Working Folder
----------------------


A $(ProjectDir)\Installation folder should always be created. 
The project assemblies need to be copied to the "Installation" folder, using build post events.

The DNNpackager is expecting the asseemblies in the Installaiton folder, so it can copy them to the website bin from there.

```

copy "$(ProjectDir)$(OutDir)$(TargetFileName)" "$(ProjectDir)\Installation\$(TargetFileName)"
copy "$(ProjectDir)$(OutDir)$(AssemblyName).pdb" "$(ProjectDir)\Installation\$(AssemblyName).pdb"

$(ProjectDir)\DNNpackager.bat

```

Running this from VS will also create an installation zip file for DNN.

