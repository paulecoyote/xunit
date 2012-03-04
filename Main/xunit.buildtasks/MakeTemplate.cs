using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

public class MakeTemplate : Task
{
    [Required]
    public ITaskItem[] InputFiles { get; set; }

    [Required]
    public ITaskItem ZipFile { get; set; }

    public override bool Execute()
    {
        string zipFilename = ZipFile.GetMetadata("FullPath");
        string zipDateString = ZipFile.GetMetadata("ModifiedTime");
        DateTime zipDate = zipDateString.Length == 0 ? DateTime.MinValue : DateTime.Parse(zipDateString);
        bool build = false;

        foreach (var item in InputFiles)
            if (DateTime.Parse(item.GetMetadata("ModifiedTime")) > zipDate)
            {
                build = true;
                break;
            }

        if (!build)
            return true;

        File.Delete(zipFilename);
        Log.LogMessage("Creating template {0}", zipFilename);

        using (var zipFile = new CodePlex.ZipLibrary.ZipFile(zipFilename))
        {
            foreach (var item in InputFiles)
            {
                string itemFilename = item.GetMetadata("FullPath");
                string directoryPathInArchive = item.GetMetadata("OutputLocation") + item.GetMetadata("RecursiveDir");
                zipFile.AddFile(itemFilename, directoryPathInArchive);
            }

            zipFile.Save();
        }

        return true;
    }
}
