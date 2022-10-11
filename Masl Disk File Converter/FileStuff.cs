using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masl_Disk_File_Converter
{
    using static Program;

    struct FSPartData
    {
        public ulong location, size;
        public bool free;
    }

    struct BaseData
    {
        public bool hidden;
        public bool sysFile;
        public bool readOnly;
        public string path;
    }

    struct FileData
    {
        public BaseData baseData;
        public ulong location;
        public ulong size;
    }

    struct FolderData
    {
        public BaseData baseData;
    }

    public class FileStuff
    {
        public static void PartToFolder(string filename, string folderName)
        {
            Println($"Reading Disk:           \"{filename}\"");
            if (Directory.Exists(folderName))
                Directory.Delete(folderName, true);

            byte[] partData = File.ReadAllBytes(filename);

            string signature = "";
            for (int i = 0; i < 7; i++)
                signature += (char)partData[i];
            Println($"> Partition Signature:  \"{signature}\"");
            if (!signature.Equals("MRAFS01"))
            {
                Println($"! Partition Signature not supported");
                return;
            }
            Directory.CreateDirectory(folderName);

            int offset = 7;

            ulong totalSize = BEC.ByteArrToUint64(partData, offset);
            offset += 8;
            uint fsPartCount = BEC.ByteArrToUint32(partData, offset);
            offset += 4;
            uint fileCount = BEC.ByteArrToUint32(partData, offset);
            offset += 4;
            uint folderCount = BEC.ByteArrToUint32(partData, offset);
            offset += 4;

            Println($"Total Partition Size: {totalSize} Bytes");
            Println($"FS Partition Count: {fsPartCount}");
            Println($"File Count: {fileCount}");
            Println($"Folder Count: {folderCount}");

            using (StreamWriter writer = new StreamWriter(folderName + "/../partInfos.txt"))
            {
                writer.WriteLine("Partition Size (Bytes):");
                writer.WriteLine(totalSize);
                writer.WriteLine("FS Partition Count:");
                writer.WriteLine(fsPartCount);
                writer.WriteLine("File Count:");
                writer.WriteLine(fileCount);
                writer.WriteLine("Folder Count:");
                writer.WriteLine(folderCount);

                Dictionary<ulong /*location*/, FSPartData> fsPartDict = new Dictionary<ulong, FSPartData>();
                List<FolderData> folders = new List<FolderData>();
                List<FileData> files = new List<FileData>();
                StartIndent("FS PART DATA");
                for (int i = 0; i < fsPartCount; i++)
                {
                    StartIndent($"FS PART {i}");
                    FSPartData current = new FSPartData();
                    current.size = BEC.ByteArrToUint64(partData, offset);
                    offset += 8;
                    current.location = BEC.ByteArrToUint64(partData, offset);
                    offset += 8;
                    current.free = partData[offset++] == 1;
                    Println($"Location: {current.location}");
                    Println($"Size: {current.size} Bytes");
                    Println($"Free: {current.free}");
                    EndIndent($"FS PART {i}");
                    fsPartDict.Add(current.location, current);
                }
                EndIndent("FS PART DATA");

                StartIndent("FILE DATA");
                for (int i = 0; i < fileCount; i++)
                {
                    StartIndent($"File {i}");
                    FileData fData = new FileData();
                    fData.baseData = new BaseData();
                    fData.baseData.hidden = partData[offset++] == 1;
                    fData.baseData.sysFile = partData[offset++] == 1;
                    fData.baseData.readOnly = partData[offset++] == 1;
                    fData.baseData.path = "";
                    ushort pathLen = BEC.ByteArrToUint16(partData, offset);
                    offset += 2;
                    for (int i2 = 0; i2 < pathLen; i2++)
                        fData.baseData.path += (char)partData[offset++];

                    fData.location = BEC.ByteArrToUint64(partData, offset);
                    offset += 8;
                    fData.size = BEC.ByteArrToUint64(partData, offset);
                    offset += 8;

                    Println($"Path:     {fData.baseData.path}");
                    Println($"Location: {fData.location}");
                    Println($"Size:     {fData.size} Bytes");

                    Println($"Hidden:   {fData.baseData.hidden}");
                    Println($"System:   {fData.baseData.sysFile}");
                    Println($"Readonly: {fData.baseData.readOnly}");

                    files.Add(fData);

                    EndIndent($"File {i}");
                }
                EndIndent("FILE DATA");


                StartIndent("FOLDER DATA");
                for (int i = 0; i < folderCount; i++)
                {
                    StartIndent($"Folder {i}");
                    FolderData fData = new FolderData();
                    fData.baseData = new BaseData();
                    fData.baseData.hidden = partData[offset++] == 1;
                    fData.baseData.sysFile = partData[offset++] == 1;
                    fData.baseData.readOnly = partData[offset++] == 1;
                    fData.baseData.path = "";
                    ushort pathLen = BEC.ByteArrToUint16(partData, offset);
                    offset += 2;
                    for (int i2 = 0; i2 < pathLen; i2++)
                        fData.baseData.path += (char)partData[offset++];

                    //fData.location = BEC.ByteArrToUint64(partData, offset);
                    //offset += 8;
                    //fData.size = BEC.ByteArrToUint64(partData, offset);
                    //offset += 8;

                    Println($"Path:     {fData.baseData.path}");
                    //Println($"Location: {fData.location}");
                    //Println($"Size:     {fData.size} Bytes");

                    Println($"Hidden:   {fData.baseData.hidden}");
                    Println($"System:   {fData.baseData.sysFile}");
                    Println($"Readonly: {fData.baseData.readOnly}");

                    folders.Add(fData);

                    EndIndent($"Folder {i}");
                }
                EndIndent("FOLDER DATA");

                foreach (FolderData folder in folders)
                {
                    string name = folderName + "/" + folder.baseData.path;
                    if (!Directory.Exists(name))
                        Directory.CreateDirectory(name);
                }

                foreach (FileData file in files)
                {
                    string name = folderName + "/" + file.baseData.path;
                    using (BinaryWriter fWriter = new BinaryWriter(new FileStream(name, FileMode.Create)))
                    {
                        if (file.size == 0)
                            continue;

                        byte[] fData = new byte[file.size];
                        for (ulong i2 = 0; i2 < file.size; i2++)
                            fData[i2] = partData[file.location + i2];

                        fWriter.Write(fData);
                    }

                    FileInfo fileInfo = new FileInfo(name);
                    
                    fileInfo.IsReadOnly = file.baseData.readOnly;

                    if (file.baseData.hidden)
                        fileInfo.Attributes |= FileAttributes.Hidden;
                    else
                        fileInfo.Attributes &= ~FileAttributes.Hidden;

                    if (file.baseData.sysFile)
                        fileInfo.Attributes |= FileAttributes.System;
                    else
                        fileInfo.Attributes &= ~FileAttributes.System;

                }
            }

        }

        public static void FolderToPart(string filename, string folderName)
        {

        }
    }
}
