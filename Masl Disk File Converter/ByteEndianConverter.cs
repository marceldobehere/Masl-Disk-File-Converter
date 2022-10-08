using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masl_Disk_File_Converter
{
    public class BEC //ByteEndianConverter
    {
        static byte[] SwitchByteArr(byte[] data, int start, int count, bool switchArr)
        {
            byte[] newData = new byte[count];
            if (!switchArr)
                for (int i = 0; i < count; i++)
                    newData[i] = data[(start + count - 1) - i];
            else
                for (int i = 0; i < count; i++)
                    newData[i] = data[start + i];

            return newData;
        }

        public static ulong ByteArrToUint64(byte[] arr, int startIndex, bool switchByteArr = true)
        {
            arr = SwitchByteArr(arr, startIndex, 8, switchByteArr);
            return BitConverter.ToUInt64(arr, 0);
        }

        public static uint ByteArrToUint32(byte[] arr, int startIndex, bool switchByteArr = true)
        {
            arr = SwitchByteArr(arr, startIndex, 4, switchByteArr);
            return BitConverter.ToUInt32(arr, 0);
        }

        public static ushort ByteArrToUint16(byte[] arr, int startIndex, bool switchByteArr = true)
        {
            arr = SwitchByteArr(arr, startIndex, 2, switchByteArr);
            return BitConverter.ToUInt16(arr, 0);
        }
    }
}
