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

The DNNpackager takes 1, 2 or 3 arguments.  

DNNpackager.exe \<ProjectDir\>\<Config Name (optional)\>

DNNpackager.exe \<ProjectDir\>\<Config Name (optional)\> \<ConfigurationName\>

DNNpackager.exe \<ProjectDir\>\<Config Name (optional)\> \<CopyDestination\> \<BinSource\> \<BinDestination\> \<ConfigurationName\>

arg1- **\<ProjectDir\>\<Config Name (optional)\>** = \$(ProjectDir)  The config .dnnpack file.  If the \<Config Name (optional)\> is omitted the project folder is searched for any ".dnnpack" file.  The .dnnpack file should be created on the project root folder.

arg2 - **\<BinSource\>** = The bin folder of the source module.  In VS tokens it is \$(ProjectDir)\$(OutDir) (optional)

arg3 - **\<ConfigurationName\>** = The VS configuration. \$(ConfigurationName) (optional)

**"/clean"** option can be added to the command.  This option will delete files on the "copy website" that do not exist in the project.  Use caution when using this, runtime files that have been added for plugin operations will be removed.

```
DNNpackager.exe $(ProjectDir) $(ProjectDir)$(OutDir) $(ConfigurationName) /clean
```

**"/debug"** option will pause the execution for 6 seconds, so the VS debugger can be attached.
```
DNNpackager.exe $(ProjectDir) $(TargetDir) $(ConfigurationName) /debug
```
**"/docs"** option will convert the markdown documentation to html. (See below)
```
DNNpackager.exe $(ProjectDir) $(TargetDir) $(ConfigurationName) /docs
```

Example from VS post build event:
```

C:\Program Files (x86)\Nevoweb\DNNpackager\DNNpackager.exe  $(ProjectDir) $(ProjectDir)$(OutDir) $(ConfigurationName)

```

If the directory of the exe is included in the windows PATH environment, you can minimise the line in VS post build event to:
```

DNNpackager.exe  $(ProjectDir) $(ProjectDir)$(OutDir) $(ConfigurationName)

```

**NOTE:** If the **\<ConfigurationName\>** is "razor" or starts with "nc-" DNNpackager will assume no compile has been made and will not copy the assemblies to the destination.


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

Any website files older than the source folder will be pulled to the git folder. 

Files that do not exist in the source, but exist on the website will have the extension removed so they are not used by the webiste.  If a file must be included then it should be manually copied to the source folder.

A $(ProjectDir)\Installation folder should always be created. 

Running DNNpacker from VS when in "release" config, will create an installation zip file for DNN.

Copy Razor templates
--------------------

When we copy razor templates we do not wish to compile and copy the assemblies.  (Doing this will cause the AppPool to recycle)

We create a config in the VS project called "Razor" (or any other name starting with "nc-") that has ALL project compile turned off.  We can use this to Sync the razor templates.  Any templates on the website folder will be copied to the Git project folder if they are newer than the Git file. And the same copy/sync rules apply for copying the Git working area files to the website.

Build Documentation
-------------------
Markdown documentation files can be automatically converted to html for documentation.  Use "\docs" option the command.

The Markdown files MUST be in a sub-folder of the project called "\docs".  

Any images used MUST be in a sub-folder of the project called "\docs\img".  

Any generated html and images will be created in the destination website under the [_metadata_:docsfolder] meta data specified in the markdown.  

The ReadMe.md on the project root will also be included.  

If the markdown does NOT have any meta data, the file will be ignored.  

###  Markdown Meta Data
The markdown meta data in a simple set of text that will be hidden.

```
[_metadata_:templatefolder]: rocketerms
[_metadata_:docsfolder]: docs\erms\advert
[_metadata_:menugroup]: Advert
[_metadata_:name]: Introduction
[_metadata_:sortorder]: 0

```
**NOTE: Token values MUST NOT have any spaces, any underscore will be converted to a space.**

### Template base & Tokens
The documentation HTML is made using a html template.  The Template MUST be called "\docs\templates\Template.html" & "\docs\templates\menuline.html".  

Tokens included will inject the [MENU] and [BODY] for the full template and [URL] and [NAME] for the Menu template.

Example Menu Line Template:
```
<a href="[URL]" class="w3-bar-item w3-button w3-hover-white">[NAME]</a>
```

Example Full Template:
```
<!DOCTYPE html>
<html lang="en">
<head>
    <title>CRS - Advert</title>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <link rel="stylesheet" href="https://www.w3schools.com/w3css/4/w3.css">
    <link rel="stylesheet" href="https://www.w3schools.com/lib/w3-theme-indigo.css">
    <link rel="stylesheet" href="https://fonts.googleapis.com/css?family=Roboto">
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/4.7.0/css/font-awesome.min.css">
    <script type="text/javascript" src="//translate.google.com/translate_a/element.js?cb=googleTranslateElementInit"></script>
    <style>
        html, body, h1, h2, h3, h4, h5, h6 {
            font-family: "Roboto", sans-serif;
        }

        .w3-sidebar {
            z-index: 3;
            width: 250px;
            top: 43px;
            bottom: 0;
            height: inherit;
        }
    </style>
</head>
<body>

    <!-- Navbar -->
    <div class="w3-top">
        <div class="w3-bar w3-theme w3-left-align w3-large w3-text-bottom" style="height:43px;">
            <div class="w3-bar-item w3-theme-l1">Central Resource System - Adverts</div>
            <div class="w3-bar-item w3-right w3-input w3-padding" id="google_translate_element"></div>
            <div class="w3-bar-item w3-button w3-right w3-hide-large w3-hover-white w3-large w3-theme-l1" onclick="w3_open()"><i class="fa fa-bars"></i></div>
        </div>
    </div>

    <!-- Sidebar -->
    <nav class="w3-sidebar w3-bar-block w3-collapse w3-large w3-theme-l5 " id="mySidebar">
        <a href="javascript:void(0)" onclick="w3_close()" class="w3-right w3-xlarge w3-padding-large w3-hover-black w3-hide-large" title="Close Menu">
            <i class="fa fa-remove"></i>
        </a>
        [MENU]
    </nav>

    <!-- Overlay effect when opening sidebar on small screens -->
    <div class="w3-overlay w3-hide-large" onclick="w3_close()" style="cursor:pointer" title="close side menu" id="myOverlay"></div>

    <!-- Main content: shift it to the right by 250 pixels when the sidebar is visible -->
    <div class="w3-main" style="margin-left:250px">

        <div class="w3-row w3-margin-top w3-padding-64">
            <div class="w3-container">
                [BODY]
            </div>
        </div>

        <footer id="myFooter">

            <div class="w3-container">
                <div class="w3-right">Powered by <a href="https://www.rocket-cds.org/" target="_blank">RocketCDS</a></div>
            </div>
        </footer>

        <!-- END MAIN -->
    </div>

    <script>

        document.addEventListener("DOMContentLoaded", () => {
            googleTranslateElementInit();
        });

// Get the Sidebar
var mySidebar = document.getElementById("mySidebar");

// Get the DIV with overlay effect
var overlayBg = document.getElementById("myOverlay");

// Toggle between showing and hiding the sidebar, and add overlay effect
function w3_open() {
  if (mySidebar.style.display === 'block') {
    mySidebar.style.display = 'none';
    overlayBg.style.display = "none";
  } else {
    mySidebar.style.display = 'block';
    overlayBg.style.display = "block";
  }
}

// Close the sidebar with the close button
function w3_close() {
  mySidebar.style.display = "none";
  overlayBg.style.display = "none";
        }

        // Google Translate
        function googleTranslateElementInit() {
            new google.translate.TranslateElement({ pageLanguage: 'en', layout: google.translate.TranslateElement.InlineLayout.SIMPLE }, 'google_translate_element');
        }


    </script>

</body>
</html>


```


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


RUN AS ADMINISTRATOR
--------------------

You should run with administrator right, the BAT file below helps to do this.  right click the BAT file and "Run as Administrator".

```
REM ******** RUN AS ADMIN ***************

DNNpackager.exe %~dp0DNNpackager.dnnpack razor

PAUSE
```

