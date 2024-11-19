using System.IO;
using System.Text;

namespace Jicoteo.Manager.Helpers
{
    public static class FileHelper
    {
        public static string GetRootDirectory(string folderName = "", string fileName = "")
        {
            var root = "";//Environment.CurrentDirectory;
            if (!string.IsNullOrWhiteSpace(folderName) && !string.IsNullOrWhiteSpace(fileName))
            {
                string withDirectory = Path.Combine(root, folderName);
                if (!Directory.Exists(withDirectory))
                {
                    Directory.CreateDirectory(withDirectory);
                }
                return Path.Combine(withDirectory, fileName);
            }
            return root;
        }

        public static string ReadTextFile(string fileName)
        {
            string text;
            using var fs = File.Open(fileName, FileMode.OpenOrCreate);
            using var sr = new StreamReader(fs, new UTF8Encoding(false));
            text = sr.ReadToEnd();
            return text;
        }

        public static string ReadTextFileFromDirectory(string dirName, string fileName)
                            => ReadTextFile(GetRootDirectory(dirName, fileName));
    }
}