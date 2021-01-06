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

DNNpackager.exe \<ProjectDir\> \<Website Module Folder\> \<Website bin Folder\> $(ConfigurationName)

**ProjectDir** =  The root folder of the source project.

**Website Module Folder** = The module folder in the website where the module files will be copied to.

**Website bin Folder** = The bin folder of the website, where the assembly needs to be copied.

Example:
```

"C:\Program Files (x86)\Nevoweb\DNNpackager\DNNpackager.exe"  "$(ProjectDir)" "C:\Nevoweb\Temp\copytest" "$(ProjectDir)$(OutDir)" "C:\Nevoweb\Temp\bin" $(ConfigurationName)

```

Running From VS
---------------
The operation is most easily ran from the "Post Build Event".  This automates the transfer of files from the working folders to the website folders.

A BAT file needs to be cretaed which specifies the path to the website

NOTE: The BAT file needs to be saved as ASCII.

Example:
```

echo off
set mpath = "C:\Nevoweb\Websites\www.dnnrocket.com\Install\DesktopModules\DNNrocketModules\RocketEcommerce"
set bpath = "C:\Nevoweb\Websites\www.dnnrocket.com\Install\bin"
C:\Nevoweb\Projects\Utils\DNNpackager\bin\netcoreapp3.1\DNNpackager.exe %1 %mpath% %2 %bpath% %3

```

Example of Post Build event in VS, which runs the BAT file:
```

$(ProjectDir)\Installation\Dist\www.dnnrocket.com.bat  $(ProjectDir) $(ProjectDir)$(OutDir) $(ConfigurationName)

```




Copy to Working Folder
----------------------

If ONLY the $(ProjectDir) argument is passed to DNNpacker, a DNN install will be created.  However, if more arguments as passed DNNpackager can move files from your repo working area to your dev website area.
This will allow you to create only 1 GIT repo on you dev machine.  Different BAT file can be created for different websites.

Files form the working area will be copied to the website folders.

A $(ProjectDir)\Installation folder should always be created. 

Running DNNpacker from VS when in "release" config, will create an installation zip file for DNN.

Copy Razor templates
--------------------

When we copy razor templates we do not wish to compile and copy the assemblies.  This ill cause the AppPool to recycle.

We create a config in the project called "Razor" that has ALL projects compile turned off.  We can use this to Sync the razorr templates.  Any templates on the website folder will be copied to the Git project folder if they are newer than the Git file. 


