using Windows.Storage;
using System.IO;
using System.Text;
using Windows.ApplicationModel;

namespace LCSC.Manager.Helpers
{
    public static class FileHelper
    {
        public static string GetFileInAppDirectoryAsync(string folderName = "", string fileName = "")
        {
            var root = Package.Current.InstalledLocation.Path;
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
                            => ReadTextFile(GetFileInAppDirectoryAsync(dirName, fileName));
    }
}