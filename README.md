# DNNpackager

This console application will copy the installation files to the website, like when the module has been installed.  

It will also package a DNN module into an Install zip file.

A configuration file should be in the Project root folder: "DNNpackager.dnnpack".

Example:
```xml
<root>
	<version>1.0.0</version>
  <websitedestrelpath>\DesktopModules\DNNrocket\AppThemes</websitedestrelpath>
  <websitedestbinrelpath>\bin</websitedestbinrelpath>
  <!-- Include only files that match the regular expression -->
	    <regexpr>(\.cshtml|\.html|\.resx|\.dnn|\.png|\.jpg|\.gif|\.css|\.svg|\.js|\.xml|\.txt|\.md|\.aspx|\.ascx|\.ashx|\.woff|\.woff2|\.ttf)$</regexpr>
	<directory include='false'>
		<!-- All paths should be from the source root (project root) -->
		<value>\.git</value>
		<value>\.vs</value>
		<value>\bin</value>
		<value>\Components</value>
		<value>\Installation\Dist</value>
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
		<value>RocketAppThemes.dll</value>
    <value>RocketAppThemes.pdb</value>
  </assembly>
</root>
```

Installation
------------

Use the Installation Package to install on the local machine or clone the repository from GitHub and compile.

Running From VS
-----------------

The operation is most easily ran from the "Post Build Event".  This automates the transfer of files from the working folders to the website folders.

The DNNpackager takes 1 or 3 arguments.  

DNNpackager.exe \<ProjectDir\>\<Config Name (optional)\>

DNNpackager.exe \<ProjectDir\>\<Config Name (optional)\> \<CopyDestination\> \<BinSource\> \<BinDestination\> \<ConfigurationName\>

arg1- **\<ProjectDir\>\<Config Name (optional)\>** = \$(ProjectDir)  The config .dnnpack file.  If the \<Config Name (optional)\> is omitted the project folder is searched for any ".dnnpack" file.  The .dnnpack file should be created on the project root folder.

arg2 - **\<BinSource\>** = The bin folder of the source module.  In VS tokens it is \$(ProjectDir)\$(OutDir) (optional)

arg3 - **\<ConfigurationName\>** = The VS configuration. \$(ConfigurationName) (optional)

Example from VS post build event:
```

C:\Program Files (x86)\Nevoweb\DNNpackager\DNNpackager.exe  $(ProjectDir) $(ProjectDir)$(OutDir) $(ConfigurationName)

```

If the directory of the exe is included in the windows PATH environment, you can minimise the line in VS post build event to:
```

DNNpackager.exe  $(ProjectDir) $(ProjectDir)$(OutDir) $(ConfigurationName)

```


dnnpack.config File
---------------

The "dnnpack.config" file is required to copy files to the development website.  (Or Sync)

Example:
```
<root>
  <websitemappath>D:\Websites\dev.rocket.com\Install</websitemappath>
</root>
```
Deprecated Example:
```
<root>
  <websitebinfoldermappath>C:\Nevoweb\Websites\www.dnnrocket.com\Install\bin</websitebinfoldermappath>
  <websitedestfoldermappath>C:\Nevoweb\Websites\www.dnnrocket.com\Install\DesktopModules\DNNrocketModules\RE_CartPriceShipping</websitedestfoldermappath>
</root>
```

If no file exists in the project a blank XML file is created.


Copy to Working Folder
----------------------

If ONLY the $(ProjectDir) argument is passed to DNNpacker, a DNN install will be created.  You can associate the .dnnpack extension with DNNpackager, you can then double click on the .dnnpack file to build the installation.

If more arguments are passed DNNpackager can move files from your repo working area to your dev website area. This will allow you to create only 1 GIT repo on you dev machine and use it on multiple websites.

Files from the working area will be copied to the website folders.

A $(ProjectDir)\Installation folder should always be created. 

Running DNNpacker from VS when in "release" config, will create an installation zip file for DNN.

Copy Razor templates
--------------------

When we copy razor templates we do not wish to compile and copy the assemblies.  (Doing this will cause the AppPool to recycle)

We create a config in the VS project called "Razor" (or any other name apart from "release" and "debug") that has ALL project compile turned off.  We can use this to Sync the razor templates.  Any templates on the website folder will be copied to the Git project folder if they are newer than the Git file. And the same copy/sync rules apply for copying the Git working area files to the website.


VS External Tool
----------------
You can also setup external tools in VS to make compiling quicker.

It is best to setup 3 tools, each for release, debug, razor.

Shortcut keys can also help.  Setup from >Tools>Options>Environment>Keyboard

Search for "External." to find the possible commands.  If you place the compile in position 1,2,3 you can link like this:

```
DNN Build (release)
DNNpackager.exe
$(ProjectDir) $(TargetDir) release
$(ProjectDir)
```
\<Shit + Ctrl + 1\>  

```
DNN Build (debug)
DNNpackager.exe
$(ProjectDir) $(TargetDir) debug
$(ProjectDir)
```
\<Shit + Ctrl + 2\>  

```
DNN Build (razor)
DNNpackager.exe
$(ProjectDir) $(TargetDir) razor
$(ProjectDir)
```
\<Shit + Ctrl + 3\>  


Working with Multiple Projects
------------------------------
Sometimes we want to work with multiple projects on the same website.  The "dnnpack.config" is looked for in the project file first, but if not found it will search in the parent folder until it finds a "dnnpack.config".

In the "dnnpack.config" file are the required website destination mappath to the website root.

```
<root>
  <websitemappath>D:\Websites\dev.rocket.com\Install</websitemappath>
</root>
```
This will be used, with the destination iformation in the "DNNpackager.dnnpack" file to move the files to the correct website folders.
```
  <websitedestrelpath>\DesktopModules\DNNrocket\AppThemes</websitedestrelpath>
  <websitedestbinrelpath>\bin</websitedestbinrelpath>
```
"websitedestbinrelpath" is optional.

Example:
```
DevFolder > Projects > Project1
DevFolder > Projects > Project2
DevFolder > Projects > Project3
```

The "dnnpack.config" file should be placed in ```DevFolder > Projects``` folder.  When each of the projects are compiled or DNNpacker run it will use the parent "dnnpack.config" (Unless you place a "dnnpack.config" file in the project folder).

The allows you to have multiple projects under 1 folder, that all use the same "dnnpack.config" file and hence the same website.




