# Convert Project from MSBUILD

MSBUILD is the Microsoft build engine and is very complete and can do anything that DNNpackager does.  

So the question is, why use DNN-packager?  

The quick answer is MSBUILD is massive and complex when you start doing seperate development folder and build of the DNN package.  

MSBUILD is difficult to setup, has a large set of scripts and often creates errors between dev systems, because of setup. This can take hours to sort out.  
DNNpackager is easier to setup and offers an external tool that is not linked to the Visual Studio project.  So to overcome setup problems, you can remove the call to DNNpackager and the project will compile as expected.  


### Step 1
Edit the "\<project>.csproj" file with notepad.

Remove line...
```
  <Import Project="BuildScripts\ModulePackage.Targets" />
```

Change line...
```
  <Target Name="AfterBuild" DependsOnTargets="PackageModule">
  </Target> 
```
to
```
  <Target Name="AfterBuild">
  </Target>
```
### Step 2
Delete the "BuildScripts" folder from the VS project.

### Step 3
Add "DNNpackager.dnnpack" as a XML file.

Example...  (Change "websitedestrelpath", "assembly", "directory" and any other)
```xml
<root>
  <version>1.0.0</version>
  <websitedestrelpath>\DesktopModules\DNNrocketModules\Espace4x</websitedestrelpath>
  <websitedestbinrelpath>\bin</websitedestbinrelpath>
  <!-- Include only files that match the regular expression -->
  <regexpr>(\.cshtml|\.html|\.resx|\.dnn|\.png|\.jpg|\.gif|\.css|\.svg|\.js|\.xml|\.txt|\.md|\.aspx|\.ascx|\.ashx|\.woff|\.woff2|\.ttf|\.rules)$</regexpr>
  <directory include='false'>
    <!-- All paths should be from the source root (project root) -->
    <value>\.git</value>
    <value>\.vs</value>
    <value>\bin</value>
    <value>\obj</value>
    <value>\Components</value>
    <value>\Documentation</value>
    <value>\Examples</value>
    <value>\Installation\Dist</value>
    <value>\packages</value>
    <value>\Render</value>
    <value>\SQL</value>
    <value>\_external</value>
  </directory>
  <file include='false'>
  </file>
  <file include='true'>
    <value></value>
  </file>
  <assembly>
    <value>Espace4x.dll</value>
    <value>Espace4x.pdb</value>
  </assembly>
</root>
```
### Step 4
Add "dnnpack.config" file.
```
<root>
  <websitemappath>C:\DevWebsites\website</websitemappath>
  <list>
  </list>  
</root>
```

### Step 5
Edit Post-build Event
```
DNNpackager.exe  $(ProjectDir) $(ProjectDir)$(OutDir) $(ConfigurationName)
```
### Step 6
Relink refs.
The VS projects inside the webstructure of often link to the bin folder or other projects for references.  These references have not changed.  

There are a number of methods to relinking these.  

The simplest is to simply create a "_external" folder in the prject and copy the required assemblies to it, the link the refs to those assemblies.

Another populate way is to link the already existing projects in the dev folder area.  Remember those project may also have a external folder, which you can link to.
