#nullable disable
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using System.Security.Cryptography;
using System.Text;

namespace Acme.FileBackup.App.Utilities
{
    static public class UtilityHelpers
    {
        static public List<string> GetExcludeList(string appPath)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(appPath, nameof(appPath));
            string excludeFile = appPath + @"\exclude.txt";
            var excludeList = new List<string>();
            if (File.Exists(excludeFile))
            {
                var fileLines = File.ReadAllLines(excludeFile);
                excludeList = new List<string>(fileLines);
                return excludeList;
            }
            return new List<string>();
        }

        static public List<string> GetExtensionsList(string appPath)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(appPath, nameof(appPath));
            string extensionsFile = appPath + @"\extensions.txt";
            var extensionsList = new List<string>();
            if (File.Exists(extensionsFile))
            {
                var fileLines = File.ReadAllLines(extensionsFile);
                extensionsList = new List<string>(fileLines);
                return extensionsList;
            }
            return new List<string>();
        }

        public static string CreateDestinationPath(FileInfo sourcfileInfo, string destinationNode, bool useMetatData, OrganizationFormat orgFormat)
        {
            ArgumentNullException.ThrowIfNull(sourcfileInfo, nameof(sourcfileInfo));
            ArgumentNullException.ThrowIfNullOrEmpty(destinationNode, nameof(destinationNode));

            DateTime fileDate;
            var souceDirectoryPath = sourcfileInfo.DirectoryName;
            var souceDirectoryPathRoot = Path.GetPathRoot(souceDirectoryPath);

            if (useMetatData)
            {
                var takenDate = GetDateTakenFromImage(sourcfileInfo.FullName);

                if (takenDate != DateTime.MinValue)
                {
                    fileDate = takenDate;
                }
                else
                {
                    fileDate = sourcfileInfo.CreationTime > sourcfileInfo.LastWriteTime
                        ? sourcfileInfo.LastWriteTime
                        : sourcfileInfo.CreationTime;
                }
            }
            else
            {
                fileDate = sourcfileInfo.CreationTime > sourcfileInfo.LastWriteTime
                    ? sourcfileInfo.LastWriteTime
                    : sourcfileInfo.CreationTime;
            }

            switch (orgFormat)
            {
                case OrganizationFormat.ByYear:
                    {
                        var year = fileDate.Year.ToString();
                        return destinationNode + "\\" + year + "\\" + sourcfileInfo.Name;
                    }
                case OrganizationFormat.ByMonth:
                    {
                        var month = fileDate.ToString("MMM");
                        var monthNumber = fileDate.ToString("MM");
                        var year = fileDate.Year.ToString();
                        return destinationNode + "\\" + year + "\\" + monthNumber + "_" + month + "\\" + sourcfileInfo.Name;
                    }
                case OrganizationFormat.ByDay:
                    {
                        var month = fileDate.ToString("MMM");
                        var monthNumber = fileDate.ToString("MM");
                        var year = fileDate.Year.ToString();
                        var dayOfMonth = fileDate.Day.ToString().PadLeft(2, '0');
                        return destinationNode + "\\" + year + "\\" + monthNumber + "_" + month + "\\" + dayOfMonth + "\\" + sourcfileInfo.Name;
                    }
                case OrganizationFormat.PathEcho:
                    {
                        int index = souceDirectoryPath.IndexOf(souceDirectoryPathRoot);
                        string cleanPath = (index < 0)
                            ? souceDirectoryPath
                            : souceDirectoryPath.Remove(index, souceDirectoryPathRoot.Length);

                        return destinationNode + "\\" + cleanPath + "\\" + sourcfileInfo.Name;
                    }
                case OrganizationFormat.Flat:
                default:
                    {
                        return destinationNode + "\\" + sourcfileInfo.Name;
                    }
            }
        }

        public static DateTime GetDateTakenFromImage(string path)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(path, nameof(path));
            var isImage = ImageFormats.IsImage(path);

            if (!isImage) return DateTime.MinValue;

            var directories = ImageMetadataReader.ReadMetadata(path);
            var subIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
            var dateTime = subIfdDirectory?.GetDescription(ExifDirectoryBase.TagDateTime);
            var dateString = dateTime ?? string.Empty;

            DateTime myDate;
            if (!DateTime.TryParse(dateString, out myDate))
            {
                myDate = DateTime.MinValue;
            }

            return myDate;
        }

        public static void CopyFileExactly(string copyFromPath, string copyToPath, string errorLogFile)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(copyFromPath, nameof(copyFromPath));
            ArgumentNullException.ThrowIfNullOrEmpty(copyToPath, nameof(copyToPath));
            ArgumentNullException.ThrowIfNullOrEmpty(errorLogFile, nameof(errorLogFile));

            var toPath = Path.GetDirectoryName(copyToPath);

            try
            {
                if (!System.IO.Directory.Exists(toPath))
                {
                    System.IO.Directory.CreateDirectory(toPath);
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(errorLogFile, copyFromPath + Environment.NewLine);
                Console.WriteLine("File {0} {1} {2}", "EXCEPTION-1", ex.Message, "");
                return;
            }

            try
            {
                var originFile = new FileInfo(copyFromPath);
                var destinationFile = new FileInfo(copyToPath);

                if (File.Exists(copyToPath))
                {
                    File.AppendAllText(toPath + "//dup.txt", copyFromPath + Environment.NewLine);
                    Console.WriteLine("File {0} {1} {2}", "DUP", copyFromPath, copyToPath);
                    return;
                }
                Console.WriteLine("File {0} {1} {2}", "COPY", copyFromPath, copyToPath);
                originFile.CopyTo(copyToPath, true);

                File.AppendAllText(toPath + "//index.txt", copyFromPath + Environment.NewLine);

                destinationFile.CreationTime = originFile.CreationTime;
                destinationFile.LastWriteTime = originFile.LastWriteTime;
                destinationFile.LastAccessTime = originFile.LastAccessTime;
            }
            catch (Exception ex)
            {
                File.AppendAllText(errorLogFile + "//error.txt", copyFromPath + Environment.NewLine);
                Console.WriteLine("File {0} {1} {2}", "EXCEPTION-2", ex.Message, "");
                return;
            }
            return;
        }

        private static string CreateMd5ForFolder(string path)
        {
            // assuming you want to include nested folders
            var files = System.IO.Directory.GetFiles(path, "*", SearchOption.AllDirectories)
                                 .OrderBy(p => p).ToList();

            MD5 md5 = MD5.Create();

            for (int i = 0; i < files.Count; i++)
            {
                string file = files[i];

                // hash path
                string relativePath = file.Substring(path.Length + 1);
                byte[] pathBytes = Encoding.UTF8.GetBytes(relativePath.ToLower());
                md5.TransformBlock(pathBytes, 0, pathBytes.Length, pathBytes, 0);

                // hash contents
                byte[] contentBytes = File.ReadAllBytes(file);
                if (i == files.Count - 1)
                    md5.TransformFinalBlock(contentBytes, 0, contentBytes.Length);
                else
                    md5.TransformBlock(contentBytes, 0, contentBytes.Length, contentBytes, 0);
            }

            return BitConverter.ToString(md5.Hash).Replace("-", "").ToLower();
        }

        private static void DisplayImageTags(string path)
        {
            Console.WriteLine($"Start Image Tags - {path} --------------------------------");
            var directories = ImageMetadataReader.ReadMetadata(path);
            foreach (var directory in directories)
            {
                Console.WriteLine(directory.Name + "   " + directory.Tags.Count.ToString() + " tags.");
                foreach (var Tag in directory.Tags)
                {
                    Console.WriteLine("     " + Tag.Name + "  :  " + Tag.Description);
                }
            }
            Console.WriteLine("END Image Tags  ---------------------------------------------------------------");
        }
    }
}
