﻿using System.Text;


namespace NotepadStateLibrary
{
    public class NPWindowState
    {
        /// <summary>
        /// Number of the active tab in Notepad. 0 based index.
        /// </summary>
        public ulong ActiveTab { get; private set; }
        /// <summary>
        /// Bottom right X/Y coordinates
        /// </summary>
        public WindowXY BottomRightCoords { get; private set; }
        /// <summary>
        /// bytes of the described tab state file
        /// </summary>
        public byte[] bytes { get; set; }
        /// <summary>
        /// Number of bytes to the CRC
        /// </summary>
        public ulong BytesToCRC { get; private set; }
        /// <summary>
        /// Calculated CRC32
        /// </summary>
        public byte[] CRC32Calculated { get; private set; } = null!;
        /// <summary>
        /// CRC32 from the file
        /// </summary>
        public byte[] CRC32Stored { get; private set; } = null!;
        /// <summary>
        /// Number of tabs in Notepad
        /// </summary>
        public ulong NumberTabs { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public ulong SequenceNumber { get; private set; }
        /// <summary>
        /// List of GUIDs stored as Bytes for Tabs. These are in order.
        /// </summary>
        public List<byte[]> Tabs { get; private set; }
        /// <summary>
        /// Top left X/Y coordinates
        /// </summary>
        public WindowXY TopLeftCoords { get; private set; }
        /// <summary>
        /// Height and Width. Should match the differences between Top left and Bottom right coordinates.
        /// </summary>
        public WindowXY WindowSize { get; private set; }

        

        /// <summary>
        /// Notepad Window State Files
        /// </summary>
        /// <param name="bytes">bytes of the state file</param>
        public NPWindowState(byte[] bytes) 
        {
            this.bytes = bytes;

            Tabs = new List<byte[]>();

            ParseBytes();
        }

        /// <summary>
        /// Sets active tab
        /// </summary>
        /// <param name="NewActiveTab">Integer of the active tab. 0 based index.</param>
        public void ChangeActiveTab(int NewActiveTab)
        {
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                using (MemoryStream outStream = new MemoryStream())
                {
                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        using (BinaryWriter writer = new BinaryWriter(outStream))
                        {
                            var hdr = reader.ReadBytes(2);
                            writer.Write(hdr);
                            string hdrType = Encoding.ASCII.GetString(hdr);
                            if (hdrType == "NP")
                            {
                                CRC32Check c = new CRC32Check();

                                reader.ReadLEB128Unsigned();
                                writer.Write(SequenceNumber.WriteLEB128Unsigned());
                                c.AddBytes(SequenceNumber);

                                reader.ReadLEB128Unsigned();
                                writer.Write(BytesToCRC); 
                                c.AddBytes(BytesToCRC);


                                var delim = reader.ReadBytes(1);
                                writer.Write(delim);
                                c.AddBytes(delim);

                                reader.ReadLEB128Unsigned();
                                writer.Write(NumberTabs.WriteLEB128Unsigned());
                                c.AddBytes(NumberTabs);

                                for (int x = 0; x < (int)NumberTabs; x++)
                                {
                                    var chunk = reader.ReadBytes(16);
                                    writer.Write(chunk);
                                    c.AddBytes(chunk);
                                }

                                reader.ReadLEB128Unsigned(); //Active Tab
                                ActiveTab = (ulong)NewActiveTab;
                                writer.Write(ActiveTab.WriteLEB128Unsigned());
                                c.AddBytes(ActiveTab);

                                var tlc1 = reader.ReadUInt32();
                                writer.Write(tlc1);
                                c.AddBytes(tlc1);
                                var tlc2 = reader.ReadUInt32();
                                writer.Write(tlc2);
                                c.AddBytes(tlc2);

                                TopLeftCoords = new WindowXY((int)tlc1, (int)tlc2);


                                var brc3 = reader.ReadUInt32();
                                writer.Write(brc3);
                                c.AddBytes(brc3);
                                var brc4 = reader.ReadUInt32();
                                writer.Write(brc4);
                                c.AddBytes(brc4);

                                BottomRightCoords = new WindowXY((int)brc3, (int)brc4);


                                var wsc5 = reader.ReadUInt32();
                                writer.Write(wsc5);
                                c.AddBytes(wsc5);
                                var wsc6 = reader.ReadUInt32();
                                writer.Write(wsc6);
                                c.AddBytes(wsc6);

                                WindowSize = new WindowXY((int)wsc5, (int)wsc6);


                                var delim2 = reader.ReadBytes(1);
                                writer.Write(delim2);
                                c.AddBytes(delim2);

                                reader.ReadBytes(4);
                                CRC32Stored = c.CRC32;
                                writer.Write(CRC32Stored);
                                CRC32Calculated = c.CRC32;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="NewTabs"></param>
        public void WriteTabList(List<byte[]> NewTabs)
        {
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                using (MemoryStream outStream = new MemoryStream())
                {
                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        using (BinaryWriter writer = new BinaryWriter(outStream))
                        {
                            var hdr = reader.ReadBytes(2);
                            writer.Write(hdr);
                            string hdrType = Encoding.ASCII.GetString(hdr);
                            if (hdrType == "NP")
                            {
                                CRC32Check c = new CRC32Check();

                                reader.ReadLEB128Unsigned();
                                writer.Write(SequenceNumber.WriteLEB128Unsigned());
                                c.AddBytes(SequenceNumber);

                                reader.ReadLEB128Unsigned();
                                writer.Write(BytesToCRC);
                                c.AddBytes(BytesToCRC);


                                var delim = reader.ReadBytes(1);
                                writer.Write(delim);
                                c.AddBytes(delim);

                                var OriginalNumberTabs = reader.ReadLEB128Unsigned();
                                NumberTabs = (ulong)NewTabs.Count;

                                writer.Write(NumberTabs.WriteLEB128Unsigned());
                                c.AddBytes(NumberTabs);

                                reader.ReadBytes((int)OriginalNumberTabs * 16);

                                for (int x = 0; x < (int)NumberTabs; x++)
                                {
                                    writer.Write(Tabs[x]);
                                    c.AddBytes(Tabs[x]);
                                }

                                reader.ReadLEB128Unsigned(); 
                                writer.Write(ActiveTab.WriteLEB128Unsigned());
                                c.AddBytes(ActiveTab);

                                var tlc1 = reader.ReadUInt32();
                                writer.Write(tlc1);
                                c.AddBytes(tlc1);
                                var tlc2 = reader.ReadUInt32();
                                writer.Write(tlc2);
                                c.AddBytes(tlc2);

                                TopLeftCoords = new WindowXY((int)tlc1, (int)tlc2);


                                var brc3 = reader.ReadUInt32();
                                writer.Write(brc3);
                                c.AddBytes(brc3);
                                var brc4 = reader.ReadUInt32();
                                writer.Write(brc4);
                                c.AddBytes(brc4);

                                BottomRightCoords = new WindowXY((int)brc3, (int)brc4);


                                var wsc5 = reader.ReadUInt32();
                                writer.Write(wsc5);
                                c.AddBytes(wsc5);
                                var wsc6 = reader.ReadUInt32();
                                writer.Write(wsc6);
                                c.AddBytes(wsc6);

                                WindowSize = new WindowXY((int)wsc5, (int)wsc6);


                                var delim2 = reader.ReadBytes(1);
                                writer.Write(delim2);
                                c.AddBytes(delim2);

                                reader.ReadBytes(4);
                                CRC32Stored = c.CRC32;
                                writer.Write(CRC32Stored);
                                CRC32Calculated = c.CRC32;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="TopLeft"></param>
        /// <param name="BottomRight"></param>
        public void Resize(WindowXY TopLeft, WindowXY BottomRight)
        {
            //This should be the main one
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="WindowSize"></param>
        public void Resize(WindowXY WindowSize)
        {
            //TODO: Update BottomRightCoords to coincide with WindowSize Change
            Resize(TopLeftCoords, BottomRightCoords);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="TopLeft"></param>
        public void Move(WindowXY TopLeft)
        {
            //TODO: Update BottomRightCoords to coincide with move
            Resize(TopLeftCoords, BottomRightCoords);
        }

        private void ParseBytes()
        {
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    string hdrType = Encoding.ASCII.GetString(reader.ReadBytes(2));

                    if (hdrType == "NP")
                    {
                        CRC32Check c = new CRC32Check();

                        SequenceNumber = reader.ReadLEB128Unsigned();
                        c.AddBytes(SequenceNumber);


                        BytesToCRC = reader.ReadLEB128Unsigned();
                        c.AddBytes(BytesToCRC);


                        var delim = reader.ReadBytes(1);
                        c.AddBytes(delim);

                        NumberTabs = reader.ReadLEB128Unsigned();
                        c.AddBytes(NumberTabs);

                        for (int x = 0; x < (int)NumberTabs; x++)
                        {
                            var chunk = reader.ReadBytes(16);
                            Tabs.Add(chunk);
                            c.AddBytes(chunk);
                        }

                        ActiveTab = reader.ReadLEB128Unsigned(); //Active Tab
                        c.AddBytes(ActiveTab);

                        var tlc1 = reader.ReadUInt32();
                        c.AddBytes(tlc1);
                        var tlc2 = reader.ReadUInt32();
                        c.AddBytes(tlc2);

                        TopLeftCoords = new WindowXY((int)tlc1, (int)tlc2);


                        var brc3 = reader.ReadUInt32();
                        c.AddBytes(brc3);
                        var brc4 = reader.ReadUInt32();
                        c.AddBytes(brc4);

                        BottomRightCoords = new WindowXY((int)brc3, (int)brc4);


                        var wsc5 = reader.ReadUInt32();
                        c.AddBytes(wsc5);
                        var wsc6 = reader.ReadUInt32();
                        c.AddBytes(wsc6);

                        WindowSize = new WindowXY((int)wsc5, (int)wsc6);


                        var delim2 = reader.ReadBytes(1);
                        c.AddBytes(delim2);


                        CRC32Stored = reader.ReadBytes(4);
                        CRC32Calculated = c.CRC32;
                    }
                }
            }
        }          
    }
}
