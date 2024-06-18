using System.IO.Hashing;

namespace NotepadStateLibrary
{
    internal class CRC32Check
    {
        private List<byte> _data;

        public byte[] CRC32 { get { var c = Crc32.Hash(_data.ToArray()); Array.Reverse(c); return c; } }

        public CRC32Check()
        {
            _data = new List<byte>();
        }

        public void AddBytes(byte[] bytes)
        {
            foreach (byte b in bytes)
            {
                _data.Add(b);
            }
        }

        public void AddBytes(ulong value)
        {
            AddBytes(value.WriteLEB128Unsigned());
        }

        public void AddBytes(uint value)
        {
            AddBytes(BitConverter.GetBytes(value));
        }
    }
}
