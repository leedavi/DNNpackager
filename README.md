# DNNpackager

This console application is designed to allow the source of a module to be edited when not included in the webite structure.
It will copy the installation files to the website, like when the module has been installed.  

It will also package a DNN module into an Install zip file.

It should also have a configuration file in the Project root folder called "DNNpackager.dnnpack".

Example:
```xml
<root>
	<version>1.0.0</version>
	<!-- Include only files that match the regular expression -->
	<regexpr>(\.cshtml|\.html|\.resx|\.dnn|\.png|\.css|\.js|\.xml|\.txt|\.md|\.aspx|\.ascx|\.ashx)$</regexpr>
	<directory include='false'>
		<!-- All paths should be from the source root (project root) -->
		<value>\.git</value>
		<value>\.vs</value>
		<value>\bin</value>
		<value>\Components</value>
		<value>\Installation</value>
		<value>\Interfaces</value>
		<value>\obj</value>
		<value>\packages</value>
		<value>\Providers</value>
		<value>\render</value>
		<value>\SqlDataProvider</value>
		<value>\_external</value>
		<value>\ApiControllers</value>    
	</directory>
	<file include='false'>
	</file>
	<assembly>
		<value>DNNrocketAPI.dll</value>
		<value>DNNrocketAPI.pdb</value>
	</assembly>
</root>
```

Installation
------------

Use the Installation Package to install on the local machine.

Create a BAT file
-----------------

The easiest way to run DNNpackager is to create a BAT file that executes the assembly when VS compiles the code.
 
The DNNpackager takes 1 or 5 arguments.  

DNNpackager.exe \<ProjectDir\>\<Config Name (optional)\>

DNNpackager.exe \<ProjectDir\>\<Config Name (optional)\> \<CopyDestination\> \<BinSource\> \<BinDestination\> \<ConfigurationName\>

arg1- **\<ProjectDir\>\<Config Name (optional)\>** =  The config .dnnpack file.  If the \<Config Name (optional)\> is omitted the project folder is searched for any ".dnnpack" file.  The .dnnpack file should be created on the project root folder.

arg2 - **\<CopyDestination\>** = The module folder in the website where the project files will be copied to. (optional)

arg3 - **\<BinSource\>** = The bin folder of the source module.  In VS tokens it is $(ProjectDir)$(OutDir) (optional)

arg3 - **\<BinDestination\>** = The bin folder of the website. Where the assembly will be copied to. (optional)

arg3 - **\<ConfigurationName\>** = The VS configuration. $(ConfigurationName) (optional)

Example from VS post build event:
```

"C:\Program Files (x86)\Nevoweb\DNNpackager\DNNpackager.exe"  "$(ProjectDir)" "C:\Nevoweb\Temp\copytest" "$(ProjectDir)$(OutDir)" "C:\Nevoweb\Temp\bin" $(ConfigurationName)

```

Running From VS
---------------
The operation is most easily ran from the "Post Build Event".  This automates the transfer of files from the working folders to the website folders.

A BAT file can be cretaed to make it easier to use on different project. It specifies the path to the website folders.

NOTE: The BAT file needs to be saved as ASCII.

Example:
```

echo off
set mpath = "C:\Nevoweb\Websites\www.dnnrocket.com\Install\DesktopModules\DNNrocketModules\RocketEcommerce"
set bpath = "C:\Nevoweb\Websites\www.dnnrocket.com\Install\bin"
"C:\Program Files (x86)\Nevoweb\DNNpackager\DNNpackager.exe" %1 %mpath% %2 %bpath% %3

```

This BAT file can then be called from the post build event in VS.  Different BAT files can be used for different websaite paths.

Example of Post Build event in VS, which runs the BAT file:
```

$(ProjectDir)\Installation\Dist\www.dnnrocket.com.bat  $(ProjectDir) $(TargetDir) $(ConfigurationName)

```



Copy to Working Folder
----------------------

If ONLY the $(ProjectDir) argument is passed to DNNpacker, a DNN install will be created.  You can associate the .dnnpack extension with DNNpackager, you can then double click on the .dnnpack file to build the installation.

If more arguments are passed DNNpackager can move files from your repo working area to your dev website area. This will allow you to create only 1 GIT repo on you dev machine.

Files from the working area will be copied to the website folders.

A $(ProjectDir)\Installation folder should always be created. 

Running DNNpacker from VS when in "release" config, will create an installation zip file for DNN.

Copy Razor templates
--------------------

When we copy razor templates we do not wish to compile and copy the assemblies.  (Doing this will cause the AppPool to recycle)

We create a config in the VS project called "Razor" (or any other name apart from "release" and "debug") that has ALL project compile turned off.  We can use this to Sync the razor templates.  Any templates on the website folder will be copied to the Git project folder if they are newer than the Git file. 


