using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masl_Disk_File_Converter
{
    public class PartStuff
    {
        public static void DiskToFolder(string filename, string folderName)
        {
            Console.WriteLine($"Reading Disk:    \"{filename}\"");
            if (Directory.Exists(folderName))
                Directory.Delete(folderName, true);
            Directory.CreateDirectory(folderName);

            byte[] diskData = File.ReadAllBytes(filename);
            //for (int i = 0; i < 100; i++)
            //    Console.Write($"{diskData[512 + i]} ");
            //Console.WriteLine();

            string signature = "";
            for (int i = 0; i < 7; i++)
                signature += (char)diskData[i + 82];
            Console.WriteLine($"Disk Signature:  \"{signature}\"");
            if (!signature.Equals("MRAPS01"))
            {
                Console.WriteLine($"Partition Signature not supported");
                return;
            }
            

            int offset = 512;

            ulong totalSize = BEC.ByteArrToUint64(diskData, offset);
            offset += 8;
            ushort partCount = BEC.ByteArrToUint16(diskData, offset);
            offset += 2;

            Console.WriteLine($"Disk Size:       {diskData.LongLength} Bytes");
            Console.WriteLine($"Total Size:      {totalSize} Bytes");
            Console.WriteLine($"Partition Count: {partCount}");

            using (StreamWriter writer = new StreamWriter($"{folderName}/diskInfos.txt"))
            {
                writer.WriteLine("Disk Size (Bytes):");
                writer.WriteLine(diskData.Length);
                writer.WriteLine("Total Partition Header Size (Bytes):");
                writer.WriteLine(totalSize);
                writer.WriteLine("Partition Count:");
                writer.WriteLine(partCount);
                writer.WriteLine("List:");
                for (int i = 0; i < partCount; i++)
                {
                    writer.WriteLine("Partition ID:");
                    writer.WriteLine(i);

                    ushort nameLen = BEC.ByteArrToUint16(diskData, offset);
                    offset += 2;
                    string name = string.Empty;
                    for (int i2 = 0; i2 < nameLen; i2++)
                        name += (char)diskData[offset++];
                    writer.WriteLine("Name:");
                    writer.WriteLine(name);

                    ushort descLen = BEC.ByteArrToUint16(diskData, offset);
                    offset += 2;
                    string desc = string.Empty;
                    for (int i2 = 0; i2 < descLen; i2++)
                        desc += (char)diskData[offset++];
                    writer.WriteLine("Description:");
                    writer.WriteLine(desc);

                    ushort driveNameLen = BEC.ByteArrToUint16(diskData, offset);
                    offset += 2;
                    string driveName = string.Empty;
                    for (int i2 = 0; i2 < driveNameLen; i2++)
                        driveName += (char)diskData[offset++];
                    writer.WriteLine("Drive Name:");
                    writer.WriteLine(driveName);

                    bool hidden = diskData[offset++] == 1;

                    ulong partSize = BEC.ByteArrToUint64(diskData, offset);
                    offset += 8;
                    writer.WriteLine("Partition Size (Bytes):");
                    writer.WriteLine(partSize);

                    ulong partLocation = BEC.ByteArrToUint64(diskData, offset);
                    offset += 8;
                    writer.WriteLine("Partition Location:");
                    writer.WriteLine(partLocation);

                    byte partType = diskData[offset++];
                    writer.WriteLine("Partition Type:");
                    writer.WriteLine(partType);


                    byte fsType = diskData[offset++];
                    writer.WriteLine("Filesystem Type:");
                    writer.WriteLine(fsType);

                    Directory.CreateDirectory(folderName + $"/{i}");
                    {
                        byte[] rawPartData = new byte[partSize];
                        for (ulong i2 = 0; i2 < partSize; i2++)
                            rawPartData[i2] = diskData[partLocation + i2];
                        using (BinaryWriter binWriter = new BinaryWriter(new FileStream(folderName + $"/{i}/raw.bin", FileMode.Create)))
                            binWriter.Write(rawPartData);

                        FileStuff.PartToFolder(folderName + $"/{i}/raw.bin", folderName + $"/{i}/data");
                    }
                }
            }



        }

        public static void FolderToDisk(string filename, string folderName)
        {
            Console.WriteLine($"Reading Folder: \"{folderName}\"");
            Directory.CreateDirectory(folderName);

        }
    }
}
