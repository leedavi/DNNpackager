using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace DNNpackager
{
    public class FileUtils
    {

        public static void SaveFile(string fullFileName, string data)
        {
            var buffer = StrToByteArray(data);
            SaveFile(fullFileName, buffer);
        }

        public static void SaveFile(string fullFileName, byte[] buffer)
        {
            if (File.Exists(fullFileName))
            {
                File.SetAttributes(fullFileName, FileAttributes.Normal);
            }
            FileStream fs = null;
            try
            {
                fs = new FileStream(fullFileName, FileMode.Create, FileAccess.Write);
                fs.Write(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                var ms = ex.ToString();
                // ignore, stop eror here, not important if locked.
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                    fs.Dispose();
                }
            }
        }

        public static string ReadFile(string filePath)
        {
            StreamReader reader = null;
            string fileContent;
            try
            {
                if (!File.Exists(filePath)) return "";
                reader = File.OpenText(filePath);
                fileContent = reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                var ms = ex.ToString();
                // ignore, stop eror here, not important if locked.
                fileContent = "";
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
            return fileContent;
        }


        public static string FormatFolderPath(string folderPath)
        {
            if (String.IsNullOrEmpty(folderPath) || String.IsNullOrEmpty(folderPath.Trim()))
            {
                return "";
            }

            return folderPath.EndsWith("/") ? folderPath : folderPath + "/";
        }

        public static byte[] StrToByteArray(string str)
        {
            if (str == null) str = "";
            var encoding = new UTF8Encoding();
            return encoding.GetBytes(str);
        }


        /// <summary>
        /// Convert input stream to UTF8 string, can be used for text files.
        /// </summary>
        /// <param name="InpStream"></param>
        /// <returns></returns>
        public static string InputStreamToString(Stream InpStream)
        {
            // Create a Stream object.
            // Find number of bytes in stream.
            var strLen = Convert.ToInt32(InpStream.Length);
            // Create a byte array.
            var strArr = new byte[strLen];
            // Read stream into byte array.
            InpStream.Read(strArr, 0, strLen);
            // Convert byte array to a text string.
            var strmContents = Encoding.UTF8.GetString(strArr);
            return strmContents;
        }

        /// <summary>
        /// Convert input stream to base-64 string, can be used for image/binary files.
        /// </summary>
        /// <param name="InpStream"></param>
        /// <returns></returns>
        public static string Base64StreamToString(Stream InpStream)
        {
            // Create a Stream object.
            // Find number of bytes in stream.
            var strLen = Convert.ToInt32(InpStream.Length);
            // Create a byte array.
            var strArr = new byte[strLen];
            // Read stream into byte array.
            InpStream.Read(strArr, 0, strLen);
            var strmContents = Convert.ToBase64String(strArr);
            return strmContents;
        }

        public static MemoryStream Base64StringToStream(string inputStr)
        {
            var myByte = Convert.FromBase64String(inputStr);
            var theMemStream = new MemoryStream();
            theMemStream.Write(myByte, 0, myByte.Length);
            return theMemStream;
        }


        public static void SaveBase64ToFile(string FileMapPath, string strBase64)
        {
            // Save the image to a file.
            var mem = Base64StringToStream(strBase64);

            FileStream outStream = File.OpenWrite(FileMapPath);
            mem.WriteTo(outStream);
            outStream.Flush();
            outStream.Close();
        }
        public static string GetBase64FromFile(string fileMapPath)
        {
            byte[] imageArray = System.IO.File.ReadAllBytes(fileMapPath);
            return Convert.ToBase64String(imageArray);
        }

        public static string ReplaceFileExt(string fileName, string newExt)
        {
            var strOut = Path.GetDirectoryName(fileName) + "\\" + Path.GetFileNameWithoutExtension(fileName) + newExt;
            return strOut;
        }
        public static string RemoveInvalidFileChars(string Str)
        {
            // This regex will include illegal chars you never dreamed of
            string illegalCharsPattern = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            Regex r = new Regex(string.Format("[{0}]", Regex.Escape(illegalCharsPattern)));
            return r.Replace(Str, "");
        }

        public static void AppendToLog(string logMapPathFolder, string logName, string logMessage)
        {
            var dstring = DateTime.Now.ToString("yyyy-MM-dd");
            var logfilename = logMapPathFolder.TrimEnd('\\') + "\\" + dstring + "_" + Path.GetFileNameWithoutExtension(logName) + ".txt";
            if (!File.Exists(logfilename)) SaveFile(logfilename, "START" + Environment.NewLine);
            using (StreamWriter w = File.AppendText(logfilename))
            {
                Log(DateTime.Now.ToString("d/MM/yyyy HH:mm:ss") + " :  " + logMessage, w);
            }

        }
        public static void Log(string logMessage, TextWriter w)
        {
            w.WriteLine($"{logMessage}");
        }

        public static bool CompareAreSame(string fileMapPath1, string fileMapPath2)
        {
            byte[] file1 = File.ReadAllBytes(fileMapPath1);
            byte[] file2 = File.ReadAllBytes(fileMapPath2);
            if (file1.Length == file2.Length)
            {
                for (int i = 0; i < file1.Length; i++)
                {
                    if (file1[i] != file2[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

    }
}
