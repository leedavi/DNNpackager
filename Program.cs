using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

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
        private static List<string> _includeFileList;        
        private static List<string> _assemblyList;        
        private static string _version;
        private static string _binfolder;
        private static string _name;
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Press any key to START");
                Console.ReadKey();

                if (args.Length >= 1)
                {
                    Console.WriteLine("DNNpackager - args[0]: " + args[0]);

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
                    Console.WriteLine("configPath: " + configPath);
                    Console.WriteLine("sourceRootPath: " + _sourceRootPath);
                    if (!File.Exists(configPath)) configPath += "\\DNNpackager.dnnpack"; // default to this config file, if we have not specified a valid file.
                    if (!File.Exists(configPath)) configPath += "\\DNNpackager.config"; // default to this config file, if we have not specified a valid file.
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
                        DirCopy(_sourceRootPath); // copy root without recursive
                        DirSearch(_sourceRootPath);

                        // get the .dnn files to the root.
                        foreach (var f in Directory.GetFiles(_resourcesPath))
                        {
                            if (Path.GetExtension(f).ToLower() == ".dnn")
                            {
                                var fullPath = Path.Combine(destPath, Path.GetFileName(f));
                                File.Copy(f, fullPath, true);
                            }
                        }

                        //ZIP resouce and delete temp folders
                        ZipFile.CreateFromDirectory(_resourcesPath, destPath + "\\Resource.zip");
                        Directory.Delete(_resourcesPath, true);

                        // Add assemblies - They are placed on the root folder.
                        var binFolder = _sourceRootPath + _binfolder;
                        foreach (var assemblyPath in _assemblyList)
                        {
                            if (assemblyPath != "")
                            {
                                var assemblyName = Path.GetFileName(assemblyPath);
                                if (assemblyName != "")
                                {
                                    var fullPath = binFolder.TrimEnd('\\') + "\\" + assemblyName;
                                    File.Copy(fullPath, destPath.TrimEnd('\\') + "\\" + assemblyName);
                                }
                            }
                        }

                        // Include specified file at root of install zip.
                        foreach (var fileIncludePath in _includeFileList)
                        {
                            if (fileIncludePath != "")
                            {
                                var fileIncludeName = Path.GetFileName(fileIncludePath);
                                if (fileIncludeName != "")
                                {
                                    var dest = Path.Combine(destPath, Path.GetFileName(fileIncludeName));
                                    File.Copy(fileIncludePath, dest, true);
                                }
                            }
                        }

                        //ZIP temp folder into package on the project install folder.
                        if (!Directory.Exists(_sourceRootPath + "\\Installation\\")) Directory.CreateDirectory(_sourceRootPath + "\\Installation\\");
                        if (_name == "") _name = dirName;
                        var zipFilePath = _sourceRootPath + "\\Installation\\" + _name + "_" + _version + "_Install.zip";
                        if (File.Exists(zipFilePath)) File.Delete(zipFilePath);
                        ZipFile.CreateFromDirectory(destPath, zipFilePath);
                        Directory.Delete(destPath, true);

                    }
                    else
                    {
                        Console.WriteLine("Config file missing: " + configPath);
                    }
                }
                Console.WriteLine("Press any key.");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.ToString());
                Console.WriteLine("Press any key.");
                Console.ReadKey();
            }
        }
        static void SetupConfig(string configPath)
        {
            try
            {
                _XmlDoc = new XmlDocument();
                _XmlDoc.Load(configPath);
                // get directory and file ignore list
                _ignoredDirList = new List<string>();
                var nodList = _XmlDoc.SelectNodes("root/directory[@include='false']/value");
                foreach (XmlNode nod in nodList)
                {
                    _ignoredDirList.Add(_sourceRootPath + "\\" + nod.InnerText.TrimStart('\\'));
                }
                _includeDirList = new List<string>();
                var nodList7 = _XmlDoc.SelectNodes("root/directory[@include='true']/value");
                foreach (XmlNode nod in nodList7)
                {
                    _includeDirList.Add(_sourceRootPath + "\\" + nod.InnerText.TrimStart('\\'));
                }
                // add all recursive folders
                for (int i = 0; i < _includeDirList.Count; i++)
                {
                    var r = _includeDirList[i];
                    if (r.EndsWith("*"))
                    {
                        _includeDirList[i] = r.Replace("\\*", "");
                        var recursiveList = GetRecursiveList(_includeDirList[i], new List<string>());
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
                    _ignoredFileList.Add(_sourceRootPath + "\\" + nod.InnerText.TrimStart('\\'));
                }
                _includeFileList = new List<string>();
                var nodList6 = _XmlDoc.SelectNodes("root/file[@include='true']/value");
                foreach (XmlNode nod in nodList6)
                {
                    _includeFileList.Add(_sourceRootPath + "\\" + nod.InnerText.TrimStart('\\'));
                }
                _assemblyList = new List<string>();
                var nodList5 = _XmlDoc.SelectNodes("root/assembly/value");
                foreach (XmlNode nod in nodList5)
                {
                    _assemblyList.Add(_sourceRootPath + "\\" + nod.InnerText.TrimStart('\\'));
                }
                var nod3 = _XmlDoc.SelectSingleNode("root/regexpr");
                _pattern = @"(\.cshtml|\.html|\.resx|\.dnn|\.png|\.css|\.js|\.xml|\.txt|\.md)$";
                if (nod3 != null) _pattern = nod3.InnerText;

                var nod4 = _XmlDoc.SelectSingleNode("root/version");
                _version = "0.0";
                if (nod4 != null) _version = nod4.InnerText;

                var nod6 = _XmlDoc.SelectSingleNode("root/binfolder");
                _binfolder = "\\..\\..\\..\\bin";
                if (nod6 != null) _binfolder = nod6.InnerText;

                var nod7 = _XmlDoc.SelectSingleNode("root/name");
                _name = "";
                if (nod7 != null) _name = nod7.InnerText;


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
                        Console.WriteLine(sDir + "   ---> " + destPath);
                        // copy required files.
                        var files = Directory.GetFiles(sDir)
                            .Where(x => Regex.IsMatch(x, _pattern))
                            .Select(x => x).ToList();

                        foreach (var item in files)
                        {
                            Console.WriteLine(item);
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
        static void DirSearch(string sDir)
        {
            try
            {
                foreach (string d in Directory.GetDirectories(sDir))
                {
                    if (!_ignoredDirList.Contains(d))
                    {
                        if (_includeDirList.Count == 0 || (_includeDirList.Contains(d)))
                        {
                            DirCopy(d);
                            DirSearch(d);
                        }
                    }
                }
            }
            catch (System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }
        }

        static List<string> GetRecursiveList(string rDir, List<string> l)
        {
            foreach (string d in Directory.GetDirectories(rDir))
            {
                l.Add(d);
                l = GetRecursiveList(d, l);
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

    }
}
