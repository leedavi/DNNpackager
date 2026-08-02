using Markdig;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Xml.Linq;

namespace DNNpackager
{
    public class Program
    {
        private static XmlDocument _XmlDoc;
        private static string _resourcesPath;
        private static string _sourceRootPath;
        private static string _pattern;
        private static List<string> _ignoredDirList;
        private static List<string> _includeDirList;        
        private static List<string> _ignoredFileList;
        private static Dictionary<string, string> _includeFileList;
        private static Dictionary<string, string> _moveDirList;
        private static List<string> _assemblyList;        
        private static string _version;
        private static string _binfolder;
        private static string _name;
        private static string _configRootPath;

        private static string _websiteFolder;
        private static List<string> _websiteFolderList;
        private static string _websitedestrelpath;
        private static string _websitedestbinrelpath;
        private static string _websiteBinFolder;
        private static string _websiteDestFolder;
        private static string _mdFolder;
        private static Dictionary<string, string> _jsonDictList;

        private static bool _nocompile;
        private static bool _debugMode;

        // options (args > 3)
        private static bool _repofilesdelete;

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("##################### START DNNpackager ##################### ");
                Console.WriteLine("NOTE: Shutdown website AppPool to prevent locked files.");

                _repofilesdelete = false; // Delete files in the website that don't exist in the repo;
                if (args.Length >= 1)
                {
                    if (args.Length == 1)
                    {
                        //Console.WriteLine("Press any key to start...");
                        //Console.ReadKey(); // wait for keypress if we run direct from File Explorer.
                    }

                    _websiteBinFolder = "";
                    _websiteDestFolder = "";
                    _nocompile = false;
                    _debugMode = false;

                    var binSource = "";
                    var configurationName = "release";
                    if (args.Length == 2)
                    {
                        configurationName = args[1].ToLower();
                    }
                    if (args.Length >= 3)
                    {
                        binSource = args[1];
                        configurationName = args[2].ToLower();
                    }
                    if (args.Contains("/debug")) _debugMode = true;
                    if (args.Contains("/clean")) _repofilesdelete = true;
                    
                    // Sleep if we need to debug, so we can attach debugger
                    if (_debugMode) Thread.Sleep(6000);

                    if (configurationName == "razor" || configurationName.StartsWith("nc-")) _nocompile = true;

                    var configPath = args[0];
                    if (Path.GetFileName(configPath) == "")
                    {
                        // Search for dnnpack file.
                        var dlist = Directory.GetFiles(configPath);
                        foreach (var f in dlist)
                        {
                            if (f.ToLower().EndsWith(".dnnpack")) configPath = f;
                        }
                    }

                    _sourceRootPath = Path.GetDirectoryName(configPath);

                    Console.WriteLine("ProjectFolder: " + _sourceRootPath);
                    Console.WriteLine("binSource: " + binSource);
                    Console.WriteLine("ConfigurationName: " + configurationName);
                    Console.WriteLine("Clean: " + _repofilesdelete.ToString());                  

                    if (Directory.Exists(_sourceRootPath) && File.Exists(configPath))
                    {
                        // Create the Temporary Folder for Building (Remove previous)
                        string dirName = new DirectoryInfo(_sourceRootPath).Name;
                        var rootFolder = AppDomain.CurrentDomain.BaseDirectory;
                        if (!Directory.Exists(rootFolder)) Directory.CreateDirectory(rootFolder);
                        var destPath = rootFolder + "\\" + dirName;
                        if (Directory.Exists(destPath)) Directory.Delete(destPath, true);
                        Directory.CreateDirectory(destPath);
                        _resourcesPath = destPath + "\\Resources";
                        if (Directory.Exists(_resourcesPath)) Directory.Delete(_resourcesPath, true);
                        Directory.CreateDirectory(_resourcesPath);

                        //setup config
                        SetupConfig(configPath);

                        // do recursive copy files
                        Console.WriteLine("--- Folder Search ---");
                        DirCopy(_configRootPath); // copy root without recursive
                        DirSearch(_configRootPath, 0);

                        var deployList = _websiteFolderList.Count > 0
                            ? _websiteFolderList.Select(wf => { var (d, b) = GetWebsitePaths(wf); return (root: wf, dest: d, bin: b); }).ToList()
                            : new List<(string root, string dest, string bin)> { (_websiteFolder, _websiteDestFolder, _websiteBinFolder) };

                        foreach (var site in deployList)
                        {
                            if (!string.IsNullOrEmpty(site.dest))
                            {
                                Console.WriteLine("##################### COPY FILES #####################  ");
                                // Copy files to working website directory
                                var diSource = new DirectoryInfo(_resourcesPath);
                                var diTarget = new DirectoryInfo(site.dest);
                                Console.WriteLine("--- Sync All : Take oldest file ---");
                                SyncAll(diSource, diTarget); // take the oldest file in GIT and on Website. usually for Razor Templates.

                                Console.WriteLine("##################### MOVE FILES #####################  ");
                                foreach (var mDir in _moveDirList)
                                {
                                    var diSource2 = new DirectoryInfo(diSource + mDir.Key);
                                    var diTarget2 = new DirectoryInfo(site.root + mDir.Value);
                                    Console.WriteLine("diSource2: " + diSource2.FullName);
                                    Console.WriteLine("diTarget2: " + diTarget2.FullName);
                                    CopyAll(diSource2, diTarget2, "release");
                                }
                            }
                        }


                        // get the .dnn files to the root.
                        var dnnFileExists = false;
                        foreach (var f in Directory.GetFiles(_resourcesPath))
                        {
                            if (Path.GetExtension(f).ToLower() == ".dnn")
                            {
                                dnnFileExists = true;
                                var fullPath = Path.Combine(destPath, Path.GetFileName(f));
                                File.Copy(f, fullPath, true);
                            }
                        }
                        if (!dnnFileExists)
                        {
                            // search the root for the dnn file
                            var dnnFile = Path.Combine(_sourceRootPath, Path.GetFileNameWithoutExtension(configPath) + ".dnn");
                            if (!File.Exists(dnnFile))
                            {
                                foreach (var f in Directory.GetFiles(_sourceRootPath))
                                {
                                    if (Path.GetExtension(f).ToLower() == ".dnn") dnnFile = Path.Combine(_sourceRootPath, Path.GetFileName(f));
                                }
                            }
                            var fullPath = Path.Combine(destPath, Path.GetFileName(dnnFile));
                            if (File.Exists(dnnFile))
                            {
                                dnnFileExists = true;
                                File.Copy(dnnFile, fullPath, true);
                            }
                        }

                        // Add assemblies - They are placed on the root folder.
                        if (!_nocompile)
                        {
                            Console.WriteLine("--- Copy Assemblies and Resource Zip ---");

                            //ZIP resouce and delete temp folders
                            ZipFile.CreateFromDirectory(_resourcesPath, destPath + "\\Resource.zip");
                            Directory.Delete(_resourcesPath, true);
                            if (!Directory.Exists(_sourceRootPath + "\\Installation\\")) Directory.CreateDirectory(_sourceRootPath + "\\Installation\\"); // Create installation folder (It should already exist)

                            var assemblyVersionRecord = "";
                            foreach (var assemblyPath in _assemblyList)
                            {
                                if (assemblyPath != "")
                                {
                                    var assemblyName = Path.GetFileName(assemblyPath);
                                    if (assemblyName != "")
                                    {
                                        var fullPath = assemblyPath;
                                        if (!File.Exists(fullPath)) fullPath = binSource.TrimEnd('\\') + "\\" + assemblyName;
                                        if (File.Exists(fullPath))
                                        {
                                            if (!fullPath.ToLower().EndsWith(".pdb") || (configurationName == "debug"))
                                            {
                                                File.Copy(fullPath, destPath.TrimEnd('\\') + "\\" + assemblyName, true);
                                                // Copy assembly to each working website bin directory
                                                foreach (var site in deployList)
                                                {
                                                    if (!string.IsNullOrEmpty(site.bin))
                                                    {
                                                        File.Copy(fullPath, site.bin.TrimEnd('\\') + "\\" + assemblyName, true);
                                                    }
                                                }
                                            }

                                            var versionInfo = FileVersionInfo.GetVersionInfo(fullPath);
                                            string version = versionInfo.FileVersion;
                                            Console.WriteLine("Assembly: " + fullPath);
                                            Console.WriteLine("Version: " + version);
                                            assemblyVersionRecord += assemblyName + " : " + version + Environment.NewLine;
                                        }
                                    }
                                }
                            }

                            // output verison xml file
                            if (configurationName.ToLower() == "release" && dnnFileExists)
                            {
                                if (_name == "") _name = dirName;
                                var versionMapPath = _sourceRootPath + "\\Installation\\" + _name + "_" + _version + "_Versions.xml";
                                //FileUtils.SaveFile(versionMapPath, assemblyVersionRecord);
                            }


                            // Include specified file at root of install zip.
                            foreach (var fileIncludePath in _includeFileList)
                            {
                                if (fileIncludePath.Key != "")
                                {
                                    var fileIncludeName = Path.GetFileName(fileIncludePath.Key);
                                    if (fileIncludeName != "")
                                    {
                                        var destPath2 = destPath;
                                        if (fileIncludePath.Value != "") destPath2 = destPath + "\\" + fileIncludePath.Value;
                                        if (!Directory.Exists(destPath2)) Directory.CreateDirectory(destPath2);
                                        var dest = Path.Combine(destPath2, Path.GetFileName(fileIncludeName));
                                        File.Copy(fileIncludePath.Key, dest, true);
                                    }
                                }
                            }

                            if (configurationName.ToLower() == "release" && !dnnFileExists)
                            {
                                Console.WriteLine("NO DNN manifesto to build install package.");
                            }

                            if (configurationName.ToLower() == "release" && dnnFileExists)
                            {
                                Console.WriteLine("--- Build Installation Package ---");
                                //ZIP temp folder into package on the project install folder.
                                if (_name == "") _name = dirName;
                                var zipFilePath = _sourceRootPath + "\\Installation\\" + _name + "_" + _version + "_Install.zip";

                                // Copy resource file
                                File.Copy(destPath + "\\Resource.zip", _sourceRootPath + "\\Installation\\Resource.zip", true);
                                // build a zip package
                                if (File.Exists(zipFilePath)) File.Delete(zipFilePath);
                                ZipFile.CreateFromDirectory(destPath, zipFilePath);
                            }
                        }

                        Directory.Delete(destPath, true);

                        // Output json files (always, when configured)
                        if (_jsonDictList.Count > 0)
                        {
                            Console.WriteLine("##################### BUILD JSON #####################  ");
                            foreach (var jsonFile in _jsonDictList)
                            {
                                var outputfile = jsonFile.Value;
                                var inputfile = jsonFile.Key;
                                BuildJsonFromCsFile(inputfile, outputfile);
                            }
                        }

                    }
                    else
                    {
                        Console.WriteLine("Config file missing: " + configPath);
                    }

                    if (_nocompile)
                    {
                        Console.WriteLine("***** SYNC FILES ONLY *******");
                    }
                }
                Console.WriteLine("##################### WEBSITE FOLDER(S) #####################  ");
                foreach (var wf in _websiteFolderList.Count > 0 ? _websiteFolderList : new List<string> { _websiteFolder })
                    Console.WriteLine(wf);
                Console.WriteLine("##################### END DNNpackager #####################  " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString());
                if (args.Length == 1)
                {
                    //Console.WriteLine("Press any key to end...");
                    //Console.ReadKey(); // wait for keypress if we run direct from File Explorer.
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.ToString());
                Console.WriteLine("ERROR: ********** Shutdown website AppPool to prevent locked files. **********");
                Thread.Sleep(10000);
                if (args.Length == 1)
                {
                    //Console.WriteLine("Press any key to end...");
                    //Console.ReadKey(); // wait for keypress if we run direct from File Explorer.
                }
            }
        }
        static void SyncAll(DirectoryInfo gitDir, DirectoryInfo webDir)
        {
            Directory.CreateDirectory(webDir.FullName);

            var gitList = new List<FileInfo>();
            var webList = new List<FileInfo>();
            var gitListNames = new List<string>();
            var webListNames = new List<string>();
            foreach (FileInfo fi in gitDir.GetFiles()) { gitList.Add(fi); gitListNames.Add(Path.GetFileName(fi.Name)); }
            foreach (FileInfo fi in webDir.GetFiles()) { webList.Add(fi); webListNames.Add(Path.GetFileName(fi.Name)); }

            // copy any newer files from website to git repo
            //foreach (var fi in webList)
            //{
            //    if (Path.GetExtension(fi.Name) == "")
            //    {
            //        Console.WriteLine("WARNING: No file extension.  (Not Processed)  " + fi.FullName);
            //    }
            //    else
            //    {
            //        if (File.Exists(Path.Combine(gitDir.FullName, fi.Name)))
            //        {
            //            var fileWebDate = fi.LastWriteTime;
            //            var fileGitDate = File.GetLastWriteTime(Path.Combine(gitDir.FullName, fi.Name));
            //            if (fileGitDate < fileWebDate)
            //            {
            //                Console.WriteLine("DO NOT Pull: " + Path.Combine(gitDir.FullName, fi.Name));
            //                //fi.CopyTo(Path.Combine(gitDir.FullName, fi.Name), true);
            //            }
            //        }
            //        else
            //        {
            //            //Console.WriteLine("Pull: " + fi.Name);
            //            //fi.CopyTo(Path.Combine(gitDir.FullName, fi.Name), true);
            //        }
            //    }
            //}
            // copy any newer files from git repo to website
            foreach (var fi in gitList)
            {
                if (Path.GetExtension(fi.Name) == "")
                {
                    Console.WriteLine("WARNING: No file extension.  (Not Processed)  " + fi.FullName);
                }
                else
                {
                    if (File.Exists(Path.Combine(webDir.FullName, fi.Name)))
                    {
                        var fileGitDate = fi.LastWriteTime;
                        var fileWebDate = File.GetLastWriteTime(Path.Combine(webDir.FullName, fi.Name));
                        if (fileGitDate > fileWebDate || _repofilesdelete)
                        {
                            Console.WriteLine("CopyTo: " + Path.Combine(webDir.FullName, fi.Name));
                            fi.CopyTo(Path.Combine(webDir.FullName, fi.Name), true);
                        }
                    }
                    else
                    {
                        Console.WriteLine("CopyTo: " + Path.Combine(webDir.FullName, fi.Name));
                        fi.CopyTo(Path.Combine(webDir.FullName, fi.Name), true);
                    }
                }

            }
            // remove any files in website that do not exists in the Git Repo
            // This process is a problem for plugins, where files may have been added to the project at runtime.
            // We therefore only delete files when the "/clean" options has been added to the command args[].
            if (_repofilesdelete)
            {
                foreach (var fi in webList)
                {
                    if (Path.GetExtension(fi.Name) == "")
                    {
                        Console.WriteLine("WARNING: No file extension.  (Not Processed)  " + fi.FullName);
                    }
                    else
                    {
                        if (!gitListNames.Contains(Path.GetFileName(fi.Name)))
                        {
                            if (Path.GetExtension(fi.Name) != "")
                            {
                                File.Delete(Path.Combine(webDir.FullName, fi.Name));
                                Console.WriteLine("Delete: " + Path.Combine(webDir.FullName, fi.Name));
                            }
                        }
                    }
                }
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in gitDir.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir = webDir.CreateSubdirectory(diSourceSubDir.Name);
                SyncAll(diSourceSubDir, nextTargetSubDir);
            }
        }
        static void CopyAll(DirectoryInfo source, DirectoryInfo target, string configurationName)
        {
            Directory.CreateDirectory(target.FullName);

            // Copy each file into the new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                if (File.Exists(Path.Combine(target.FullName, fi.Name)))
                {
                    var fileSourceDate = File.GetLastWriteTime(Path.Combine(source.FullName, fi.Name));
                    var fileDestDate = File.GetLastWriteTime(Path.Combine(target.FullName, fi.Name));
                    if (fileSourceDate > fileDestDate || configurationName == "release")
                    {
                        Console.WriteLine("Overwrite: " + Path.Combine(target.FullName, fi.Name));
                        fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
                    }
                }
                else
                {
                    Console.WriteLine("Copy: " + Path.Combine(target.FullName, fi.Name));
                    fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
                }
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir, configurationName);
            }
        }
        static string GetdnnpackFileMapPath(string dnnpackMapPath, int level)
        {
            if (level > 10) return "";
            level += 1;
            if (!File.Exists(dnnpackMapPath) && Directory.Exists(Path.GetDirectoryName(dnnpackMapPath)))
            {
                var newPath = Path.GetDirectoryName(dnnpackMapPath) + "\\..\\dnnpack.config";
                dnnpackMapPath = GetdnnpackFileMapPath(newPath, level);
            }
            return dnnpackMapPath;
        }

        static (string websiteDestFolder, string websiteBinFolder) GetWebsitePaths(string websiteFolder)
        {
            var websiteDestFolder = "";
            var websiteBinFolder = "";
            if (!string.IsNullOrEmpty(websiteFolder) && !string.IsNullOrEmpty(_websitedestrelpath))
            {
                websiteBinFolder = websiteFolder.TrimEnd('\\') + "\\" + _websitedestbinrelpath.Replace("/", "\\").TrimStart('\\');
                websiteDestFolder = websiteFolder.TrimEnd('\\') + "\\" + _websitedestrelpath.Replace("/", "\\").TrimStart('\\');
            }
            if (string.IsNullOrEmpty(websiteBinFolder)) websiteBinFolder = websiteFolder + _websitedestbinrelpath;
            if (string.IsNullOrEmpty(websiteDestFolder) && !string.IsNullOrEmpty(_websitedestrelpath)) websiteDestFolder = websiteFolder + _websitedestrelpath;
            return (websiteDestFolder, websiteBinFolder);
        }

        static void SetupConfig(string configPath)
        {
            try
            {
                _XmlDoc = new XmlDocument();
                _XmlDoc.Load(configPath);

                // Resolve <rootfolder> — overrides _sourceRootPath as the base for relative paths
                _configRootPath = _sourceRootPath;
                var nodRootFolder = _XmlDoc.SelectSingleNode("root/rootfolder");
                if (nodRootFolder != null && !string.IsNullOrWhiteSpace(nodRootFolder.InnerText))
                {
                    var rf = nodRootFolder.InnerText.Trim();
                    _configRootPath = Path.IsPathRooted(rf)
                        ? rf
                        : Path.GetFullPath(Path.Combine(_sourceRootPath, rf));
                    Console.WriteLine("RootFolder override: " + _configRootPath);
                }

                // get directory and file ignore list
                _ignoredDirList = new List<string>();
                var nodList = _XmlDoc.SelectNodes("root/directory[@include='false']/value");
                foreach (XmlNode nod in nodList)
                {
                    _ignoredDirList.Add(_configRootPath + "\\" + nod.InnerText.TrimStart('\\'));
                }
                _includeDirList = new List<string>();
                var nodList7 = _XmlDoc.SelectNodes("root/directory[@include='true']/value");
                foreach (XmlNode nod in nodList7)
                {
                    _includeDirList.Add(_configRootPath + "\\" + nod.InnerText.TrimStart('\\'));
                }
                // add all recursive folders
                for (int i = 0; i < _includeDirList.Count; i++)
                {
                    var r = _includeDirList[i];
                    if (r.EndsWith("*"))
                    {
                        _includeDirList[i] = r.Replace("\\*", "");
                        var recursiveList = GetRecursiveList(_includeDirList[i], new List<string>(),0);
                        foreach (var r2 in recursiveList)
                        {
                            _includeDirList.Add(r2);
                        }
                    }
                }


                _ignoredFileList = new List<string>();
                var nodList2 = _XmlDoc.SelectNodes("root/file[@include='false']/value");
                foreach (XmlNode nod in nodList2)
                {
                    _ignoredFileList.Add(_configRootPath + "\\" + nod.InnerText.TrimStart('\\'));
                }
                _includeFileList = new Dictionary<string,string>();
                var nodList6 = _XmlDoc.SelectNodes("root/file[@include='true']/value");
                foreach (XmlNode nod in nodList6)
                {
                    var dest = "";
                    if (nod.Attributes["dest"] != null) dest = nod.Attributes["dest"].InnerText;
                    _includeFileList.Add(_configRootPath + "\\" + nod.InnerText.TrimStart('\\'), dest);
                }
                _assemblyList = new List<string>();
                var nodList5 = _XmlDoc.SelectNodes("root/assembly/value");
                foreach (XmlNode nod in nodList5)
                {
                    _assemblyList.Add(_configRootPath + "\\" + nod.InnerText.TrimStart('\\'));
                }
                var nod3 = _XmlDoc.SelectSingleNode("root/regexpr");
                _pattern = @"(\.cshtml|\.html|\.resx|\.dnn|\.png|\.css|\.js|\.xml|\.txt|\.md)$";
                if (nod3 != null) _pattern = nod3.InnerText;

                var nod4 = _XmlDoc.SelectSingleNode("root/version");
                _version = "0.0";
                if (nod4 != null) _version = nod4.InnerText;

                var nod6 = _XmlDoc.SelectSingleNode("root/websitedestrelpath");
                if (nod6 != null) _websitedestrelpath = nod6.InnerText;

                var nod7 = _XmlDoc.SelectSingleNode("root/name");
                _name = "";
                if (nod7 != null) _name = nod7.InnerText;
                
                var nod8 = _XmlDoc.SelectSingleNode("root/websitedestbinrelpath");
                _websitedestbinrelpath = "\\bin";
                if (nod8 != null) _websitedestbinrelpath = nod8.InnerText;

                var nod9 = _XmlDoc.SelectSingleNode("root/docsfolder");
                if (nod9 != null) 
                    _mdFolder = nod9.InnerText;
                else
                    _mdFolder = _sourceRootPath + "\\docs";

                _moveDirList = new Dictionary<string, string>();
                var nodList10 = _XmlDoc.SelectNodes("root/movedir/value");
                foreach (XmlNode nod in nodList10)
                {
                    var dest = "";
                    if (nod.Attributes["dest"] != null) dest = nod.Attributes["dest"].InnerText;
                    _moveDirList.Add("\\" + nod.InnerText.TrimStart('\\'), dest);
                }

                

                Console.WriteLine("MarkDown Docs: " + _mdFolder.ToString());


                // load config (destination website)
                var dnnpackconfig = GetdnnpackFileMapPath(Path.GetDirectoryName(configPath) + "\\dnnpack.config", 0);
                if (!File.Exists(dnnpackconfig))
                {
                    var xmlData = "<root>" + Environment.NewLine + "<websitemappath></websitemappath>" + Environment.NewLine + "</root>";
                    File.WriteAllText(Path.GetDirectoryName(configPath) + "\\dnnpack.config", xmlData);
                }
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(dnnpackconfig);

                _websiteFolderList = new List<string>();
                var websiteFolderNodes = xmlDoc.SelectNodes("root/websitemappath");
                foreach (XmlNode wn in websiteFolderNodes)
                {
                    if (!string.IsNullOrWhiteSpace(wn.InnerText))
                        _websiteFolderList.Add(wn.InnerText);
                }
                if (_websiteFolderList.Count > 0) _websiteFolder = _websiteFolderList[0];
                //Console.WriteLine("WebsiteFolder: " + _websiteFolder);
                if (_websiteFolderList.Count == 0)
                {
                    // fallback: use explicit websitebinfoldermappath / websitedestfoldermappath
                    XmlNode websiteBinFolder = xmlDoc.SelectSingleNode("root/websitebinfoldermappath");
                    if (websiteBinFolder != null) _websiteBinFolder = websiteBinFolder.InnerText;
                    XmlNode websiteDestFolder = xmlDoc.SelectSingleNode("root/websitedestfoldermappath");
                    if (websiteDestFolder != null) _websiteDestFolder = websiteDestFolder.InnerText;
                    // if we still don't have binfolder or destfolder, build them from relpaths and websitepath
                    if (string.IsNullOrEmpty(_websiteBinFolder)) _websiteBinFolder = _websiteFolder + _websitedestbinrelpath;
                    if (string.IsNullOrEmpty(_websiteDestFolder) && !string.IsNullOrEmpty(_websitedestrelpath)) _websiteDestFolder = _websiteFolder + _websitedestrelpath;
                }
                //Console.WriteLine("WebsiteBinFolder: " + _websiteBinFolder);
                //Console.WriteLine("WebsiteDestFolder: " + _websiteDestFolder);


                _jsonDictList = new Dictionary<string, string>();
                var nodListjson = _XmlDoc.SelectNodes("root/json/file");
                foreach (XmlNode nod in nodListjson)
                {
                    var inputNod = nod.SelectSingleNode("input");
                    var outputNod = nod.SelectSingleNode("output");
                    if (inputNod != null && outputNod != null)
                    {
                        var k = inputNod.InnerText;
                        var v = outputNod.InnerText;
                        if (!_jsonDictList.ContainsKey(k)) _jsonDictList.Add(k, v);
                    }
                }


            }
            catch (System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }
        }
        static void DirCopy(string sDir)
        {
            try
            {
                if (!_ignoredDirList.Contains(sDir))
                {
                    if (_includeDirList.Count == 0 || (_includeDirList.Contains(sDir)))
                    {
                        var destPath = _resourcesPath + sDir.Replace(_sourceRootPath, "");
                        // copy required files.
                        var files = Directory.GetFiles(sDir)
                            .Where(x => Regex.IsMatch(x, _pattern))
                            .Select(x => x).ToList();

                        foreach (var item in files)
                        {
                            //Console.WriteLine(item);
                            string name = item.Substring(item.LastIndexOf("\\") + 1);
                            var fullPath = Path.Combine(destPath, name);
                            var directory = Path.GetDirectoryName(fullPath);
                            Directory.CreateDirectory(directory);
                            File.Copy(item, fullPath);
                        }
                    }
                }
            }
            catch (System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }
        }
        static void DirSearch(string sDir, int idx)
        {
            try
            {
                foreach (string d in Directory.GetDirectories(sDir))
                {
                    if (!_ignoredDirList.Contains(d))
                    {
                        if (_includeDirList.Count == 0 || (_includeDirList.Contains(d)))
                        {
                            if (idx == 0) Console.WriteLine(d);
                            DirCopy(d);
                            DirSearch(d, idx + 1);
                        }
                    }
                }
            }
            catch (System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }
        }

        static List<string> GetRecursiveList(string rDir, List<string> l, int idx)
        {
            foreach (string d in Directory.GetDirectories(rDir))
            {
                l.Add(d);
                l = GetRecursiveList(d, l, idx + 1);
            }
            return l;
        }
        public static string ReplaceLastOccurrence(string Source, string Find, string Replace)
        {
            int place = Source.LastIndexOf(Find);

            if (place == -1)
                return Source;

            string result = Source.Remove(place, Find.Length).Insert(place, Replace);
            return result;
        }

        static void BuildJsonFromCsFile(string inputFile, string outputFile)
        {
            Console.WriteLine("BuildJson: " + inputFile);
            if (!File.Exists(inputFile))
            {
                Console.WriteLine("JSON input file not found: " + inputFile);
                return;
            }

            var lines = File.ReadAllLines(inputFile);
            var namespaceName = "";
            var className = "";
            var methods = new List<Dictionary<string, object>>();
            var pendingDocLines = new List<string>();
            var isObsolete = false;

            var docLineRegex = new Regex(@"^\s*///");
            var attributeLineRegex = new Regex(@"^\s*\[");
            var namespaceRegex = new Regex(@"^\s*namespace\s+([\w.]+)");
            var classRegex = new Regex(@"^\s*(public|internal|private)\s+((?:(?:static|abstract|sealed|partial)\s+)*)class\s+(\w+)");
            var methodRegex = new Regex(@"^\s*(public)\s+((?:(?:static|virtual|override|async|abstract|new|sealed|partial|extern)\s+)*)((?:[\w<>\[\]?.,]|\s(?!\s*[({]))+?)\s+(\w+)\s*\(([^)]*)\)\s*[{;]?\s*$");

            foreach (var rawLine in lines)
            {
                var line = rawLine.TrimEnd('\r');

                var nsMatch = namespaceRegex.Match(line);
                if (nsMatch.Success && namespaceName == "")
                    namespaceName = nsMatch.Groups[1].Value.Trim();

                var classMatch = classRegex.Match(line);
                if (classMatch.Success)
                    className = classMatch.Groups[3].Value.Trim();

                if (docLineRegex.IsMatch(line))
                {
                    pendingDocLines.Add(line);
                    continue;
                }

                if (attributeLineRegex.IsMatch(line))
                {
                    if (Regex.IsMatch(line, @"^\s*\[Obsolete")) isObsolete = true;
                    continue;
                }

                var methodMatch = methodRegex.Match(line);
                if (methodMatch.Success && !isObsolete)
                {
                    var modifiers = methodMatch.Groups[2].Value;
                    var isStatic = modifiers.Contains("static");
                    var returnType = methodMatch.Groups[3].Value.Trim();
                    var methodName = methodMatch.Groups[4].Value.Trim();
                    var paramsStr = methodMatch.Groups[5].Value.Trim();

                    if (methodName != className) // skip constructors
                    {
                        var (summary, paramDocs, _) = ParseDocComments(pendingDocLines);
                        var parameters = ParseMethodParameters(paramsStr, paramDocs);
                        var signature = line.Trim().TrimEnd('{').TrimEnd(';').Trim();

                        methods.Add(new Dictionary<string, object>
                        {
                            ["type"] = "csharp-method",
                            ["name"] = methodName,
                            ["source_file"] = inputFile,
                            ["description"] = summary,
                            ["signature"] = signature,
                            ["classname"] = className,
                            ["isstatic"] = isStatic ? "true" : "false",
                            ["namespace"] = namespaceName,
                            ["parameters"] = parameters,
                            ["returns"] = returnType
                        });
                    }
                }

                pendingDocLines.Clear();
                isObsolete = false;
            }

            var json = System.Text.Json.JsonSerializer.Serialize(methods, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            var outputDir = Path.GetDirectoryName(outputFile);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);
            File.WriteAllText(outputFile, json);
            Console.WriteLine("JSON written: " + outputFile);
        }

        static (string summary, Dictionary<string, string> paramDocs, string returnDoc) ParseDocComments(List<string> docLines)
        {
            var summary = new StringBuilder();
            var paramDocs = new Dictionary<string, string>();
            var returnDoc = "";
            var inSummary = false;

            foreach (var rawLine in docLines)
            {
                var trimmed = rawLine.TrimStart();
                if (trimmed.StartsWith("///")) trimmed = trimmed.Substring(3).Trim();

                if (!inSummary && trimmed.Contains("<summary>"))
                {
                    inSummary = true;
                    var afterOpen = trimmed.Substring(trimmed.IndexOf("<summary>") + 9);
                    if (afterOpen.Contains("</summary>"))
                    {
                        summary.Append(afterOpen.Substring(0, afterOpen.IndexOf("</summary>")).Trim());
                        inSummary = false;
                    }
                    else if (afterOpen.Trim().Length > 0)
                        summary.Append(afterOpen.Trim() + " ");
                    continue;
                }
                if (inSummary)
                {
                    if (trimmed.Contains("</summary>"))
                    {
                        summary.Append(trimmed.Substring(0, trimmed.IndexOf("</summary>")).Trim());
                        inSummary = false;
                    }
                    else
                        summary.Append(trimmed + " ");
                    continue;
                }

                var paramMatch = Regex.Match(trimmed, @"<param\s+name=""(\w+)""[^>]*>(.*?)(?:</param>|$)");
                if (paramMatch.Success)
                    paramDocs[paramMatch.Groups[1].Value] = paramMatch.Groups[2].Value.Trim();

                var retMatch = Regex.Match(trimmed, @"<returns>(.*?)(?:</returns>|$)");
                if (retMatch.Success)
                    returnDoc = retMatch.Groups[1].Value.Trim();
            }

            return (summary.ToString().Trim(), paramDocs, returnDoc);
        }

        static List<Dictionary<string, string>> ParseMethodParameters(string paramsStr, Dictionary<string, string> paramDocs)
        {
            var result = new List<Dictionary<string, string>>();
            if (string.IsNullOrWhiteSpace(paramsStr)) return result;

            foreach (var part in SplitParams(paramsStr))
            {
                var p = Regex.Replace(part.Trim(), @"^(out|ref|in|params)\s+", "");
                var eqIdx = p.IndexOf('=');
                if (eqIdx > 0) p = p.Substring(0, eqIdx).Trim();
                var lastSpace = p.LastIndexOf(' ');
                if (lastSpace > 0)
                {
                    var paramType = p.Substring(0, lastSpace).Trim();
                    var paramName = p.Substring(lastSpace + 1).Trim();
                    result.Add(new Dictionary<string, string>
                    {
                        ["name"] = paramName,
                        ["type"] = paramType,
                        ["description"] = paramDocs.ContainsKey(paramName) ? paramDocs[paramName] : ""
                    });
                }
            }
            return result;
        }

        static List<string> SplitParams(string paramsStr)
        {
            var result = new List<string>();
            int depth = 0;
            int start = 0;
            for (int i = 0; i < paramsStr.Length; i++)
            {
                if (paramsStr[i] == '<') depth++;
                else if (paramsStr[i] == '>') depth--;
                else if (paramsStr[i] == ',' && depth == 0)
                {
                    result.Add(paramsStr.Substring(start, i - start));
                    start = i + 1;
                }
            }
            result.Add(paramsStr.Substring(start));
            return result;
        }
    }
}
