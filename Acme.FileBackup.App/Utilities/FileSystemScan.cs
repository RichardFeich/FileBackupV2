namespace Acme.FileBackup.App.Utilities
{
    public static class FileSystemScan
    {
        public static List<FileInfo> files = new List<FileInfo>();  // List that will hold the files and subfiles in path
        public static List<DirectoryInfo> folders = new List<DirectoryInfo>(); // List that hold direcotries that cannot be accessed
        public static List<DirectoryInfo> inaccessfolders = new List<DirectoryInfo>();
        public static int DirectoryCount;

        public static void FullDirList(string? sourceNode, string searchPattern, List<string> excludeList)
        {
            if (string.IsNullOrEmpty(sourceNode))
            {
                Console.WriteLine("ERROR - Application path could not be found");
                Console.WriteLine("*** END FILE BACKUP ***");
                Console.Read();
                Environment.Exit(0);
            }
            ArgumentNullException.ThrowIfNullOrEmpty(searchPattern, nameof(searchPattern));
            ArgumentNullException.ThrowIfNull(excludeList, nameof(excludeList));

            DirectoryInfo dir = new DirectoryInfo(sourceNode);
            DirectoryCount++;

            try
            {
                if (DirectoryCount % 1000 == 0 && DirectoryCount != 0)
                {
                    Console.WriteLine("--- Directories: {0} -- {1}", DirectoryCount, DateTime.Now);
                }

                if (excludeList.Count > 0)
                {
                    var containsItem = excludeList.Any(item => item == dir.FullName);
                    if (containsItem) { return; }
                }

                foreach (FileInfo file in dir.GetFiles(searchPattern))
                {
                    //System.Console.WriteLine("File {0} {1}", f.FullName, f.CreationTime);
                    files.Add(file);
                }
            }
            catch (Exception ex)
            {
                inaccessfolders.Add(dir);
                Console.WriteLine("Directory {0}  \n could not be accessed!!!!", dir.FullName);
                Console.Write(ex.ToString());
                return;  // We alredy had an error trying to access dir so dont try to access it again
            }

            // process each directory recursively
            foreach (DirectoryInfo directory in dir.GetDirectories())
            {
                folders.Add(directory);
                FullDirList(directory.FullName, searchPattern, excludeList);
            }
        }
    }
}
