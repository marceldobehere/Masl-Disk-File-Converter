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
        private static int indent = 0;
        public static void Println(string str)
        {
            Console.WriteLine(new String(' ', indent * 4) + str);
        }
        public static void StartIndent(string tag)
        {
            Println($"<{tag}>");
            indent++;
        }
        public static void EndIndent(string tag)
        {
            indent--;
            Println($"</{tag}>");
        }

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

            StartIndent("DISK");
            if (reply == "I")
                PartStuff.FolderToDisk(args[0], folderName);
            else if (reply == "F")
                PartStuff.DiskToFolder(args[0], folderName);
            EndIndent("DISK");


            Console.WriteLine("\n\nEnd.");
            Console.ReadLine();
            return;
        }
    }
}
