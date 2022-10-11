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

        public byte[] rawData;
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

                    FileInfo fileInfo = new FileInfo(name);

                    fileInfo.IsReadOnly = folder.baseData.readOnly;

                    if (folder.baseData.hidden)
                        fileInfo.Attributes |= FileAttributes.Hidden;
                    else
                        fileInfo.Attributes &= ~FileAttributes.Hidden;

                    if (folder.baseData.sysFile)
                        fileInfo.Attributes |= FileAttributes.System;
                    else
                        fileInfo.Attributes &= ~FileAttributes.System;
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

        private static string PathToRelativePath(string path, string basePath)
        {
            //Println($"Base: {basePath}");
            //Println($"Path: {path}");

            string temp = path.Substring(basePath.Length + 1).Replace('\\', '/');
            //Println($"Temp: {temp}");

            return temp;
        }

        private static FileData FileToInfo(string path, string basePath)
        {
            FileData info = new FileData();

            info.rawData = File.ReadAllBytes(path);
            info.size = (ulong)info.rawData.LongLength;
            if (info.size == 0)
                info.location = 0;

            FileInfo fileInfo = new FileInfo(path);

            info.baseData.hidden = fileInfo.Attributes.HasFlag(FileAttributes.Hidden);
            info.baseData.sysFile = fileInfo.Attributes.HasFlag(FileAttributes.System);
            info.baseData.readOnly = fileInfo.IsReadOnly;

            info.baseData.path = PathToRelativePath(path, basePath);


            return info;
        }

        private static FolderData FolderToInfo(string path, string basePath)
        {
            FolderData info = new FolderData();

            FileInfo fileInfo = new FileInfo(path);

            info.baseData.hidden = fileInfo.Attributes.HasFlag(FileAttributes.Hidden);
            info.baseData.sysFile = fileInfo.Attributes.HasFlag(FileAttributes.System);
            info.baseData.readOnly = fileInfo.IsReadOnly;

            info.baseData.path = PathToRelativePath(path, basePath);


            return info;
        }

        private static void AddRecursive(List<FolderData> folders, List<FileData> files, string folderPath, string basePath)
        {
            foreach (string file in Directory.GetFiles(folderPath))
                files.Add(FileToInfo(file, basePath));

            foreach (string folder in Directory.GetDirectories(folderPath))
            {
                folders.Add(FolderToInfo(folder, basePath));
                AddRecursive(folders, files, folder, basePath);
            }
        }

        private const ulong _maxFSTableSize = 5000000;

        public static void FolderToPart(string filename, string folderName)
        {
            byte[] rawData = File.ReadAllBytes(filename);
            for (long i = 0; i < rawData.LongLength; i++)
                rawData[i] = 0;

            List<FolderData> folders = new List<FolderData>();
            List<FileData> files = new List<FileData>();

            AddRecursive(folders, files, folderName, folderName);

            Println($"File count:   {files.Count}");
            Println($"Folder count: {folders.Count}");



            string signature = "MRAFS01";
            for (int i = 0; i < 7; i++)
                rawData[i] = (byte)signature[i];
            Println($"Partition Signature:  \"{signature}\"");

            ulong totalLen = 7 + 8 + 4 + 4 + 4;

            //Part Data
            totalLen += (8 + 8 + 1); // FS Table
            int partCount = 2; // FS TABLE + EMPTY
            ulong lastDiskMemIndex = _maxFSTableSize;
            for (int i = 0; i < files.Count; i++)
                if (files[i].size != 0)
                {
                    totalLen += (8 + 8 + 1); // FS Table
                    partCount++;
                    {
                        FileData temp = files[i];
                        temp.location = lastDiskMemIndex;
                        files[i] = temp;
                    }
                    lastDiskMemIndex += files[i].size;
                }
            totalLen += (8 + 8 + 1); // EMPTY SEG

            Println($"Last Part Index: {lastDiskMemIndex}");

            for (int i = 0; i < files.Count; i++)
                totalLen += (1 + 1 + 1 + 2 + (ulong)files[i].baseData.path.Length) + 8 + 8;
            for (int i = 0; i < folders.Count; i++)
                totalLen += (1 + 1 + 1 + 2 + (ulong)folders[i].baseData.path.Length);


            Println($"Total Lenght: {totalLen} Bytes");

            int offset = 7; // signature
            offset += BEC.ByteArrIntoArr(rawData, BEC.Uint64ToByteArr(totalLen), offset);
            offset += BEC.ByteArrIntoArr(rawData, BEC.Uint32ToByteArr((uint)partCount), offset);
            offset += BEC.ByteArrIntoArr(rawData, BEC.Uint32ToByteArr((uint)files.Count), offset);
            offset += BEC.ByteArrIntoArr(rawData, BEC.Uint32ToByteArr((uint)folders.Count), offset);


            {
                // FS TABLE
                {
                    offset += BEC.ByteArrIntoArr(rawData, BEC.Uint64ToByteArr(_maxFSTableSize), offset);
                    offset += BEC.ByteArrIntoArr(rawData, BEC.Uint64ToByteArr(0), offset);
                    rawData[offset++] = 0;
                }

                // FILES
                for (int i = 0; i < files.Count; i++)
                {
                    if (files[i].size == 0)
                        continue;

                    offset += BEC.ByteArrIntoArr(rawData, BEC.Uint64ToByteArr(files[i].size), offset);
                    offset += BEC.ByteArrIntoArr(rawData, BEC.Uint64ToByteArr(files[i].location), offset);
                    rawData[offset++] = 0;
                }

                // EMPTY
                {
                    offset += BEC.ByteArrIntoArr(rawData, BEC.Uint64ToByteArr((ulong)rawData.LongLength - lastDiskMemIndex), offset);
                    offset += BEC.ByteArrIntoArr(rawData, BEC.Uint64ToByteArr(lastDiskMemIndex), offset);
                    rawData[offset++] = 1;
                }
            }

            for (int i = 0; i < files.Count; i++)
            {
                FileData info = files[i];
                rawData[offset++] = (byte)(info.baseData.hidden ? 1 : 0);
                rawData[offset++] = (byte)(info.baseData.sysFile ? 1 : 0);
                rawData[offset++] = (byte)(info.baseData.readOnly ? 1 : 0);
                offset += BEC.ByteArrIntoArr(rawData, BEC.Uint16ToByteArr((ushort)info.baseData.path.Length), offset);
                for (int i2 = 0; i2 < info.baseData.path.Length; i2++)
                    rawData[offset++] = (byte)info.baseData.path[i2];

                offset += BEC.ByteArrIntoArr(rawData, BEC.Uint64ToByteArr(info.location), offset);
                offset += BEC.ByteArrIntoArr(rawData, BEC.Uint64ToByteArr(info.size), offset);

                for (ulong i2 = 0; i2 < info.size; i2++)
                    rawData[info.location + i2] = info.rawData[i2];
                
            }

            for (int i = 0; i < folders.Count; i++)
            {
                FolderData info = folders[i];
                rawData[offset++] = (byte)(info.baseData.hidden ? 1 : 0);
                rawData[offset++] = (byte)(info.baseData.sysFile ? 1 : 0);
                rawData[offset++] = (byte)(info.baseData.readOnly ? 1 : 0);
                offset += BEC.ByteArrIntoArr(rawData, BEC.Uint16ToByteArr((ushort)info.baseData.path.Length), offset);
                for (int i2 = 0; i2 < info.baseData.path.Length; i2++)
                    rawData[offset++] = (byte)info.baseData.path[i2];
            }

            Println($"Offset:       {offset} Bytes");


            File.WriteAllBytes(filename, rawData);
        }
    }
}
