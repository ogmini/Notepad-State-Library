using System;
using System.Collections.Generic;
using System.Linq;
namespace NotepadStateLibrary
{
    public static class LEB128Converter
    {
        public static ulong ReadLEB128Unsigned(this Stream stream)
        {
            ulong value = 0;
            int shift = 0;
            bool more = true;

            while (more)
            {
                var next = stream.ReadByte();
                if (next < 0) { throw new InvalidOperationException("Unexpected end of stream"); }

                byte b = (byte)next;

                more = (b & 0x80) != 0;   
                ulong chunk = b & 0x7fUL; 
                value |= chunk << shift;
                shift += 7;
            }

            return value;
        }

        public static ulong ReadLEB128Unsigned(this BinaryReader reader)
        {
            ulong value = 0;
            var shift = 0;
            var more = true;

            while (more)
            {
                var next = reader.ReadByte();

                more = (next & 0x80) != 0;  
                var chunk = next & 0x7fUL; 
                value |= chunk << shift;
                shift += 7;
            }

            return value;
        }

        public static byte[] WriteLEB128Unsigned(this ulong value)
        {
            byte[] bArray = new byte[0];

            bool more = true;

            while (more)
            {
                byte chunk = (byte)(value & 0x7fUL); 
                value >>= 7;

                more = value != 0;
                if (more) { chunk |= 0x80; } 

                bArray = AddByteToArray(bArray, chunk);

            };

            Array.Reverse(bArray);

            return bArray;
        }

        private static byte[] AddByteToArray(byte[] bArray, byte newByte)
        {
            byte[] newArray = new byte[bArray.Length + 1];
            bArray.CopyTo(newArray, 1);
            newArray[0] = newByte;
            return newArray;
        }
    }
}
