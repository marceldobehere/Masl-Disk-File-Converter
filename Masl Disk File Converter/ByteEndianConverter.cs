using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masl_Disk_File_Converter
{
    public class BEC //ByteEndianConverter
    {
        static byte[] SwitchByteArr(byte[] data)
        {
            byte[] newData = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
                newData[i] = data[(data.Length - i) - 1];

            return newData;
        }

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

        public static byte[] Uint64ToByteArr(ulong val, bool switchByteArr = true)
        {
            byte[] arr = BitConverter.GetBytes(val);
            if (switchByteArr)
                arr = SwitchByteArr(arr);
            return arr;
        }
        public static byte[] Uint32ToByteArr(uint val, bool switchByteArr = true)
        {
            byte[] arr = BitConverter.GetBytes(val);
            if (switchByteArr)
                arr = SwitchByteArr(arr);
            return arr;
        }
        public static byte[] Uint16ToByteArr(ushort val, bool switchByteArr = true)
        {
            byte[] arr = BitConverter.GetBytes(val);
            if (switchByteArr)
                arr = SwitchByteArr(arr);
            return arr;
        }

        public static int ByteArrIntoArr(byte[] baseArr, byte[] dataArr, int startIndex, bool switchByteArr = true)
        {
            if (switchByteArr)
                dataArr = SwitchByteArr(dataArr);

            for (int i = 0; i < dataArr.Length; i++)
                baseArr[i + startIndex] = dataArr[i];

            return dataArr.Length;
        }
    }
}
