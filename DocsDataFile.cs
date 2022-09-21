﻿using Markdig;
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
                string htmltext = MarkDownParse(markDown);
                HtmlText = templateBase.Replace("[BODY]", htmltext).Replace("[SUBMENU]", DocsBuildSubMenu(markDown));
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
                            rtn.Add(s[1].TrimEnd(']'), s[2].TrimStart('-').Trim());
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
        public void SaveHtml(string docsDestFolder)
        {
            var folder = docsDestFolder.TrimEnd('\\') + "\\" + DocsFolder;
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            FileUtils.SaveFile(folder + "\\" + Path.GetFileNameWithoutExtension(FileMapPath) + ".html", HtmlText);
        }
        public string DocsBuildSubMenu(string markDownText)
        {
            return "Sub-Menu";
        }

        public bool Exists { set; get; }
        public string FileMapPath { set; get; }
        public string HtmlText { set; get; }
        public string TemplateFolder { get { if (MetaData.ContainsKey("templatefolder")) return MetaData["templatefolder"]; else return ""; } }
        public string DocsFolder { get { if (MetaData.ContainsKey("docsfolder")) return MetaData["docsfolder"]; else return ""; } }
        public string Url { get { return "/RocketDocs/" + MetaData["docsfolder"].ToLower().Replace("\\","/") + "/" + Path.GetFileNameWithoutExtension(FileMapPath) + ".html"; } }
        public string SortOrder { get { if (MetaData.ContainsKey("sortorder")) return MetaData["sortorder"]; else return ""; } }
        public string Name { get { if (MetaData.ContainsKey("name")) return MetaData["name"]; else return ""; } }
        public string MenuGroup { get { if (MetaData.ContainsKey("menugroup")) return MetaData["menugroup"]; else return ""; } }  
        public Dictionary<string, string> MetaData { set; get; }

    }
}
