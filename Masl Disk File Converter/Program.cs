using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masl_Disk_File_Converter
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Invalid Arguments!");
                Console.ReadLine();
                return;
            }
            string folderName = Path.GetFileNameWithoutExtension(args[0]);
            
            string reply = "A";
            while (!"IF".Contains(reply))
            {
                Console.WriteLine($"IMG->FOLDER(F) OR FOLDER->IMG(I)?");
                reply = Console.ReadLine();
            }

            if (reply == "I")
                FolderToDisk(args[0], folderName);
            else if (reply == "F")
                DiskToFolder(args[0], folderName);
            



            Console.WriteLine("\n\nEnd.");
            Console.ReadLine();
            return;
        }

        static void DiskToFolder(string filename, string folderName)
        {
            Console.WriteLine($"Reading Disk: \"{filename}\"");
            if (Directory.Exists(folderName))
                Directory.Delete(folderName, true);
            Directory.CreateDirectory(folderName);

            byte[] diskData = File.ReadAllBytes(filename);
            
            ulong totalSize = BitConverter.ToUInt64(diskData, 0);
            ushort partCount = BitConverter.ToUInt16(diskData, 8);
            

            using (StreamWriter writer = new StreamWriter($"{folderName}/diskInfos.txt"))
            {
                writer.WriteLine("Disk Size (Bytes):");
                writer.WriteLine(diskData.Length);
                writer.WriteLine("Total Partition Header Size (Bytes):");
                writer.WriteLine(totalSize);
                writer.WriteLine("Partition Count:");
                writer.WriteLine(partCount);
            }
            
            
            
        }
        
        static void FolderToDisk(string filename, string folderName)
        {
            Console.WriteLine($"Reading Folder: \"{folderName}\"");
            Directory.CreateDirectory(folderName);
            
        }
    }
}
