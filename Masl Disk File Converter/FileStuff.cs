using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masl_Disk_File_Converter
{
    public class FileStuff
    {
        public static void PartToFolder(string filename, string folderName)
        {
            Console.WriteLine($"Reading Disk:    \"{filename}\"");
            if (Directory.Exists(folderName))
                Directory.Delete(folderName, true);
            Directory.CreateDirectory(folderName);

            byte[] diskData = File.ReadAllBytes(filename);
        }

        public static void FolderToPart(string filename, string folderName)
        {

        }
    }
}
