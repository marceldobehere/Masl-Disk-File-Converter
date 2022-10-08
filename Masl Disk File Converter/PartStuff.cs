using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masl_Disk_File_Converter
{
    using static Program;

    public class PartStuff
    {
        public static void DiskToFolder(string filename, string folderName)
        {
            Println($"Reading Disk:    \"{filename}\"");
            if (Directory.Exists(folderName))
                Directory.Delete(folderName, true);
            Directory.CreateDirectory(folderName);

            byte[] diskData = File.ReadAllBytes(filename);
            
            string signature = "";
            for (int i = 0; i < 7; i++)
                signature += (char)diskData[i + 82];
            Println($"Disk Signature:  \"{signature}\"");
            if (!signature.Equals("MRAPS01"))
            {
                Println($"Partition Signature not supported");
                return;
            }
            

            int offset = 512;

            ulong totalSize = BEC.ByteArrToUint64(diskData, offset);
            offset += 8;
            ushort partCount = BEC.ByteArrToUint16(diskData, offset);
            offset += 2;

            Println($"Disk Size:       {diskData.LongLength} Bytes");
            Println($"Total Size:      {totalSize} Bytes");
            Println($"Partition Count: {partCount}");

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

                        StartIndent("PART");
                        FileStuff.PartToFolder(folderName + $"/{i}/raw.bin", folderName + $"/{i}/data");
                        EndIndent("PART");
                    }
                }
            }



        }

        public static void FolderToDisk(string filename, string folderName)
        {
            Println($"Reading Folder: \"{folderName}\"");
            Directory.CreateDirectory(folderName);

        }
    }
}
