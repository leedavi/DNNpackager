using Markdig;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reflection.Metadata;
using System.Text;

namespace DNNpackager
{
    public class MarkDownLimpet
    {
        private string _projectFolder;
        private Dictionary<string,string> _mdMeta;
        private Dictionary<string, DocsDataFile> _htmlFiles;

        public MarkDownLimpet(string projectFolder)
        {
            _projectFolder = projectFolder;

            var templateBaseMapPath = projectFolder.TrimEnd('\\') + "\\docs\\template\\template.html";
            var templateBase = FileUtils.ReadFile(templateBaseMapPath);

            var docsList = new List<string>();
            var docsFiles = new List<DocsDataFile>();
            var docsArray = Directory.GetFiles(projectFolder.TrimEnd('\\') + "\\docs", "*.md");
            var mdFileMapPath = projectFolder.TrimEnd('\\') + "\\ReadMe.md";
            foreach (var f in docsArray)
            {
                docsList.Add(f);
            }
            docsList.Add(mdFileMapPath);
            foreach (var f in docsList)
            {
                if (File.Exists(f))
                {
                    try
                    {
                        var markDown = FileUtils.ReadFile(f);
                        var df = new DocsDataFile(f, templateBase);
                        if (df.Exists) docsFiles.Add(df);
                    }
                    catch (Exception ex)
                    {                        
                        FileUtils.SaveFile(projectFolder.TrimEnd('\\') + "\\DocsError.html", "INVALID MarkDown: " + mdFileMapPath + Environment.NewLine + " " + ex.ToString());
                    }
                }
            }
            DocsFiles = docsFiles;
        }
        public void SaveDocs(string websiteDestFolder)
        {
            var docsDestFolder = websiteDestFolder + "\\RocketDocs";
            if (!Directory.Exists(docsDestFolder)) Directory.CreateDirectory(docsDestFolder);
            foreach (var d in DocsFiles)
            {
                d.SaveHtml(docsDestFolder);
            }
        }
        public List<DocsDataFile> DocsFiles { set; get; }

    }
}
