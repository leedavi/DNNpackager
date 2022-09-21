﻿using Markdig;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;

namespace DNNpackager
{
    public class MarkDownLimpet
    {
        public MarkDownLimpet(string projectFolder)
        {
            var templateBaseMapPath = projectFolder.TrimEnd('\\') + "\\docs\\templates\\template.html";
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
            var menuHtml = DocsBuildMenu();
            foreach (var d in DocsFiles)
            {
                d.HtmlText = d.HtmlText.Replace("[MENU]", menuHtml);
                d.SaveHtml(docsDestFolder);
            }
        }
        public void SaveDoc(DocsDataFile doc, string websiteDestFolder)
        {
            var docsDestFolder = websiteDestFolder + "\\RocketDocs";
            if (!Directory.Exists(docsDestFolder)) Directory.CreateDirectory(docsDestFolder);
            doc.SaveHtml(docsDestFolder);
        }
        public List<string> GetGroups()
        {
            var rtn = new List<string>();
            foreach (var item in DocsFiles)
            {
                if (!rtn.Contains(item.MenuGroup)) rtn.Add(item.MenuGroup);
            }
            return rtn;
        }
        public List<DocsDataFile> GetDocs()
        {
            var rtn = new List<DocsDataFile>();
            var items = DocsFiles.OrderBy(x => x.SortOrder);
            foreach (var item in items)
                rtn.Add(item);
            return rtn;
        }
        public List<DocsDataFile> GetGroupDocs(string groupName)
        {
            var rtn = new List<DocsDataFile>();
            var items = DocsFiles.Where(x => x.MenuGroup == groupName).OrderBy(x => x.SortOrder); 
            foreach (var item in items)
                rtn.Add(item);
            return rtn;
        }
        public string DocsBuildMenu()
        {
            var rtn = "";
            foreach (var f in GetDocs())
            {
                rtn += "<a href=\"" + f.Url + "\" class=\"w3-bar-item w3-button w3-hover-white\">" + Path.GetFileNameWithoutExtension(f.Name).Replace("_", "&nbsp;") + "</a>";
            }
            return rtn;
        }
        public List<DocsDataFile> DocsFiles { set; get; }

    }
}
