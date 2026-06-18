Build JSON
----------

JSON files for intellisesne can be automatically created.  

In the *.dnnpack config file add a "json" node that defines each file with what output file we want.

```
<json>
	<file>
		<input>D:\NEVOWEB\Project\DesktopModules\DNNrocket\API\Components\DNNrocketUtils.cs</input>
		<output>D:\NEVOWEB\Project\DesktopModules\DNNrocket\Documentation\razortokens\DNNrocketUtils.json</output>
	</file>
	<file>
		<input>D:\NEVOWEB\Project\DesktopModules\DNNrocket\API\render\DNNrocketTokens.cs</input>
		<output>D:\NEVOWEB\Project\DesktopModules\DNNrocket\Documentation\razortokens\DNNrocketTokens.json</output>
	</file>
</json>
```

Each file listed should be a code file, the program will process the code files to make a json summary of each 

Any methods as marked as [Obsolete] will be ignored.
Any methods private methods will be ignored.


The format of the json will be multiple nodes in list:

Example: Handle404Exception method.  

```
[
  {
    "type": "csharp-method",
    "name": "Handle404Exception",
    "source_file": "API/Components/DNNrocketUtils.cs",
    "description": "Handles a 404 Not Found error by redirecting to the portal's defined 404 page or by returning a standard 404 response.",
    "signature": "public static void Handle404Exception(HttpResponse response, PortalSettings portalSetting)",
    "classname": "DNNrocketUtils",
    "isstatic": "true",
    "namespace": "DNNrocketAPI.Components",
    "parameters": [
      {
        "name": "response",
        "type": "HttpResponse",
        "description": "The current HTTP response object."
      },
      {
        "name": "portalSetting",
        "type": "PortalSettings",
        "description": "The settings of the current portal."
      }
    ],
    "returns": "void"
  }
]
```
