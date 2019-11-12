using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIDIPlayer
{
    class Utils
    {
        public static string ToBinaryString(byte myByte, int desiredLength)
        {
            string unpaddedBitString = Convert.ToString(myByte, 2);
            return unpaddedBitString.PadLeft(desiredLength, '0');
        }

        public static int BytesToInt(byte[] bytes)
        {
            if (bytes.Length < 4)
            {
                int toAdd = 4 - bytes.Length;
                List<byte> temp = new List<byte>();
                for (int i = 0; i < toAdd; i++)
                {
                    temp.Add(0x00);
                }
                for (int i = 0; i < bytes.Length; i++)
                {
                    temp.Add(bytes[i]);
                }
                bytes = temp.ToArray();
            }

            if (BitConverter.IsLittleEndian)
            {
                return BitConverter.ToInt32(bytes.Reverse().ToArray(), 0);
            }
            else
            {
                return BitConverter.ToInt32(bytes, 0);
            }
        }

        public static int GetDeltaTime(BinaryReader reader)
        {
            //  The last byte of a delta-time is identified by having MSBit = 0 
            List<byte> deltaTimeBytes = new List<byte>();
            byte currentDeltaTime = reader.ReadByte();
            deltaTimeBytes.Add(currentDeltaTime);
            while (ToBinaryString(currentDeltaTime, 8)[0] != '0')
            {
                currentDeltaTime = reader.ReadByte();
                deltaTimeBytes.Add(currentDeltaTime);
            }
            return BytesToInt(deltaTimeBytes.ToArray());
        }

        public static long ReadVariableLength(BinaryReader reader)
        {
            if (reader.BaseStream.Position != reader.BaseStream.Length)
            {

                long result = 0;
                long next = reader.ReadByte();
                result = result << 7;
                result = result | (next & 0x7F);

                while ((next & 0x80) == 0x80)
                {
                    next = reader.ReadByte();
                    result = result << 7;
                    result = result | (next & 0x7F);
                }
                //if (result != 0)
                //Debug.Log($"Variable Length: {result}");
                return result;
            }
            else return 0;
        }
    }
}
