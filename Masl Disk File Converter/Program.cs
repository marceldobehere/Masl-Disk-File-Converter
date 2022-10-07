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
                PartStuff.FolderToDisk(args[0], folderName);
            else if (reply == "F")
                PartStuff.DiskToFolder(args[0], folderName);




            Console.WriteLine("\n\nEnd.");
            Console.ReadLine();
            return;
        }
    }
}
