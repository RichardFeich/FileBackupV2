#nullable disable
using Acme.FileBackup.App.Utilities;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace Acme.Filebackup.App;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("*** START FILE BACKUP ***");

        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json");
        var configuration = builder.Build();

        Assembly assembly = Assembly.GetExecutingAssembly();
        string appPath = Path.GetDirectoryName(assembly?.Location) ?? "";
        Console.WriteLine("--- Application Path: " + appPath);
        if (string.IsNullOrEmpty(appPath)) 
        {
            Console.WriteLine("ERROR - Application path could not be found");
            Console.WriteLine("*** END FILE BACKUP ***");
            Console.Read();
            Environment.Exit(0);
        }

        var sourceNode = configuration["SourceNode"];
        var destinationNode = configuration["DestinationNode"];
        var useMetadata = configuration["UseMetadata"] == "true" ? true : false;
        var organizationFormat = (OrganizationFormat)Enum.Parse(typeof(OrganizationFormat), configuration["OrganizationFormat"]);

        var logFile = destinationNode + @"\log.txt";
        var errorLogFile = destinationNode + @"\error.txt";

        var excludeList = UtilityHelpers.GetExcludeList(appPath);
        var extensionsList = UtilityHelpers.GetExtensionsList(appPath);

        FileSystemScan.FullDirList(sourceNode, "*", excludeList);

        Console.WriteLine("Total File Count: {0}", FileSystemScan.files.Count);
        Console.WriteLine("Directory Count: {0}", FileSystemScan.DirectoryCount);
        Console.WriteLine("Folder Count: {0}", FileSystemScan.folders.Count);
        Console.WriteLine("Inaccessible Folder Count: {0}", FileSystemScan.inaccessfolders.Count);

        var fileList = FileSystemScan.files.Where(s => extensionsList.Contains(s.Extension.ToLower())).ToList();

        Console.WriteLine("Copy File Count: {0}", fileList.Count);

        File.WriteAllLines(destinationNode + "//" + @"FilesToCopy.txt", fileList.Select(a => a.FullName).ToList());
        File.WriteAllLines(destinationNode + "//" + @"InaccessibleFolders.txt", FileSystemScan.inaccessfolders.Select(a => a.FullName).ToList());

        foreach (var file in fileList)
        {
            var destinationPath = UtilityHelpers.CreateDestinationPath(file, destinationNode, useMetadata, organizationFormat);
            UtilityHelpers.CopyFileExactly(file.FullName, destinationPath, errorLogFile);
            //System.Console.WriteLine("File {0} {1} {2}", info.Name, info.CreationTime, fullDestPath);
        }
        Console.WriteLine("*** END FILE BACKUP ***");
        Console.Read();
    }
}
