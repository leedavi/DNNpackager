Build Documentation (Not Supported)
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

