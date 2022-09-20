using Markdig;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DNNpackager
{
    public class DocsDataFile
    {
        public DocsDataFile(string mdFileMapPath, string templateBase)
        {
            Exists = false;
            FileMapPath = mdFileMapPath;
            if (File.Exists(FileMapPath)) Exists = true;
            MetaData = GetMeta();
            if (MetaData.Count > 0)
            {
                var markDown = FileUtils.ReadFile(FileMapPath);
                var subMenu = DocsBuildSubMenu(markDown);
                var docsMenu = DocsBuildMenu(markDown);
                string htmltext = MarkDownParse(markDown);
                HtmlText = templateBase.Replace("[MENU]", docsMenu).Replace("[BODY]", htmltext).Replace("[SUBMENU]", subMenu);
            }
            else
            {
                Exists = false;
                HtmlText = "";
            }
        }
        private Dictionary<string, string> GetMeta()
        {
            string line = "";
            var rtn = new Dictionary<string, string>();
            using (var reader = new System.IO.StreamReader(FileMapPath))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Trim().StartsWith("[_metadata_:"))
                    {
                        var s = line.Split(':');
                        if (s.Length == 3)
                        {
                            rtn.Add(s[1].TrimEnd(']'), s[2]);
                        }
                    }
                }
            }
            return rtn;
        }
        private string MarkDownParse(string markdown)
        {
            if (string.IsNullOrEmpty(markdown))
                return "";

            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

            return Markdown.ToHtml(markdown, pipeline);
        }
        public string DocsBuildMenu(string docsFolder)
        {
            if (!Directory.Exists(docsFolder)) return "";
            var rtn = "";
            foreach (var f in Directory.GetFiles(docsFolder, "*.md"))
            {
                rtn += "<a href=\"#\" class=\"w3-bar-item w3-button w3-hover-white\">" + Path.GetFileNameWithoutExtension(f).Replace("_", "&nbsp;") + "</a>";
            }
            return rtn;
        }
        public string DocsBuildSubMenu(string markDownText)
        {
            return "Sub-Menu";
        }
        public void SaveHtml(string docsDestFolder)
        {
            FileUtils.SaveFile(docsDestFolder.TrimEnd('\\') + "\\" + DocsFolder + "\\" + Path.GetFileNameWithoutExtension(FileMapPath) + ".html", HtmlText);
        }

        public bool Exists { set; get; }
        public string FileMapPath { set; get; }
        public string HtmlText { set; get; }
        public string TemplateFolder { get { return MetaData["templatefolder"]; } }
        public string DocsFolder { get { return MetaData["docsfolder"]; } }
        public string SortOrder { get { return MetaData["sortorder"]; } }
        public string Name { get { return MetaData["name"]; } }
        public string MenuGroup { get { return MetaData["menugroup"]; } }        
        public Dictionary<string, string> MetaData { set; get; }

    }
}
