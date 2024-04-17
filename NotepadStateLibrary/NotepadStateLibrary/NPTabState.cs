using System.Reflection.PortableExecutable;
using System.Text;


namespace NotepadStateLibrary
{
    public class NPTabState
    {
        /// <summary>
        /// Size of the associated bin file in bytes for the 0.bin or 1.bin file.
        /// </summary>
        public ulong BinSize { get; private set; }
        /// <summary>
        /// bytes of the described tab state file
        /// </summary>
        public byte[] bytes { get; set; } 
        /// <summary>
        /// <para>Windows (CRLF) = 0x01</para>
        /// <para>Macintosh (CR) = 0x02</para>
        /// <para>Unix (LF) = 0x03</para>
        /// </summary>
        public byte[] CarriageReturnType { get; private set; } = null!;
        /// <summary>
        /// Content of the text file
        /// </summary>
        public byte[] Content { get; private set; } = null!;
        /// <summary>
        /// Length of the content
        /// </summary>
        public List<ulong> ContentLength { get; private set; }
        /// <summary>
        /// Calculated CRC32
        /// </summary>
        public byte[] CRC32Calculated { get; private set; } = null!;
        /// <summary>
        /// CRC32 from the file
        /// </summary>
        public byte[] CRC32Stored { get; private set; } = null!;
        /// <summary>
        /// <para>ANSI = 0x01</para>
        /// <para>UTF16LE = 0x02</para>
        /// <para>UTF16BE = 0x03</para>
        /// <para>UTF8BOM = 0x04</para>
        /// <para>UTF8 = 0x05</para>
        /// </summary>
        public byte[] EncodingType { get; private set; } = null!;
        /// <summary>
        /// SHA256 Hash of the text file saved on disk
        /// </summary>
        public byte[] FileHashStored { get; private set; } = null!;
        /// <summary>
        /// Length of the text file saved on disk
        /// </summary>
        public ulong FilePathLength { get; private set; }
        /// <summary>
        /// Path to the text file saved on disk
        /// </summary>
        public string FilePath { get; private set; } = null!;
        /// <summary>
        /// 
        /// </summary>
        public byte[] RightToLeft { get; private set; } = null!;
        /// <summary>
        /// Size of text file saved on disk
        /// </summary>
        public ulong SavedFileContentLength { get; private set; }
        /// <summary>
        /// End position of text selection
        /// </summary>
        public ulong SelectionEndIndex { get; private set; }
        /// <summary>
        /// Start position of text selection
        /// </summary>
        public ulong SelectionStartIndex { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public ulong SequenceNumber { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public byte[] ShowUnicode { get; private set; } = null!;
        /// <summary>
        /// Modified Time of Saved File
        /// </summary>
        public DateTime Timestamp { get; private set; }
        /// <summary>
        /// <para>0 = Unsaved Tab</para>
        /// <para>1 = Saved Tab</para>
        /// <para>Other = State File</para>
        /// </summary>
        public ulong TypeFlag { get; private set; }
        /// <summary>
        ///
        /// </summary>
        public byte[] Unsaved { get; private set; } = null!;
        /// <summary>
        /// Buffer of unsaved changes.
        /// </summary>
        public List<UnsavedBufferChunk> UnsavedBufferChunks { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public byte[] WordWrap { get; private set; } = null!;

        /// <summary>
        /// Notepad Tab State Files
        /// </summary>
        /// <param name="bytes">Bytes of the state file</param>
        public NPTabState(byte[] bytes)
        {
            this.bytes = bytes;

            ContentLength = new List<ulong>();
            UnsavedBufferChunks = new List<UnsavedBufferChunk>();

            ParseBytes();
        }

        /// <summary>
        /// Unsaved Tab
        /// </summary>
        /// <param name="content"></param>
        /// <param name="rightToLeft"></param>
        /// <param name="showUnicode"></param>
        /// <param name="unsavedBufferChunks"></param>
        /// <param name="wordWrap"></param>
        public NPTabState(byte[] content, byte[] rightToLeft, byte[] showUnicode, List<UnsavedBufferChunk> unsavedBufferChunks, byte[] wordWrap)
        {
            bytes = [];
            ContentLength = new List<ulong>();
            UnsavedBufferChunks = new List<UnsavedBufferChunk>();

            Content = content;
            ContentLength.Add((ulong)content.Length / 2); //TODO: Verify
            RightToLeft = rightToLeft;
            SequenceNumber = 0;
            ShowUnicode = showUnicode;
            TypeFlag = 0;
            Unsaved = [0x01]; 
            UnsavedBufferChunks = unsavedBufferChunks;
            WordWrap = wordWrap;

            Save();
        }

        /// <summary>
        /// Saved Tab
        /// </summary>
        /// <param name="sequenceNumber"></param>
        /// <param name="unsaved"></param>
        /// <param name="filePath"></param>
        /// <param name="savedFileContentLength"></param>
        /// <param name="encodingType"></param>
        /// <param name="carriageReturnType"></param>
        /// <param name="timeStamp"></param>
        /// <param name="fileHashStored"></param>
        /// <param name="selectionStartIndex"></param>
        /// <param name="selectionEndIndex"></param>
        /// <param name="wordWrap"></param>
        /// <param name="rightToLeft"></param>
        /// <param name="showUnicode"></param>
        /// <param name="content"></param>
        public NPTabState(ulong sequenceNumber, byte[] unsaved, string filePath, ulong savedFileContentLength, byte[] encodingType, 
            byte[] carriageReturnType, DateTime timeStamp, byte[] fileHashStored, ulong selectionStartIndex, ulong selectionEndIndex, byte[] wordWrap, 
            byte[] rightToLeft, byte[] showUnicode, byte[] content)
        {
            bytes = [];
            ContentLength = new List<ulong>();
            UnsavedBufferChunks = new List<UnsavedBufferChunk>();

            TypeFlag = 1;
            SequenceNumber = sequenceNumber;
            Unsaved = unsaved;
            FilePathLength = (ulong)filePath.Length;
            FilePath = filePath;
            SavedFileContentLength = savedFileContentLength;
            EncodingType = encodingType;
            CarriageReturnType = carriageReturnType;
            Timestamp = timeStamp;
            FileHashStored = fileHashStored;
            SelectionStartIndex = selectionStartIndex;
            SelectionEndIndex = selectionEndIndex;
            WordWrap = wordWrap;
            RightToLeft = rightToLeft;
            ShowUnicode = showUnicode;
            ContentLength.Add((ulong)content.Length / 2); //TODO: Verify
            Content = content;

            Save();
        }

        /// <summary>
        /// State File
        /// </summary>
        /// <param name="sequenceNumber"></param>
        /// <param name="binSize"></param>
        /// <param name="selectionStartIndex"></param>
        /// <param name="selectionEndIndex"></param>
        /// <param name="wordWrap"></param>
        /// <param name="rightToLeft"></param>
        /// <param name="showUnicode"></param>
        public NPTabState(ulong sequenceNumber, ulong binSize, ulong selectionStartIndex, ulong selectionEndIndex, byte[] wordWrap,
            byte[] rightToLeft, byte[] showUnicode)
        {
            bytes = [];
            ContentLength = new List<ulong>();
            UnsavedBufferChunks = new List<UnsavedBufferChunk>();

            SequenceNumber = sequenceNumber;
            BinSize = binSize;
            SelectionStartIndex = selectionStartIndex;
            SelectionEndIndex = selectionEndIndex;
            WordWrap = wordWrap;
            RightToLeft = rightToLeft;
            ShowUnicode = showUnicode;

            TypeFlag = (ulong)BinSize.WriteLEB128Unsigned().Length + (ulong)SelectionStartIndex.WriteLEB128Unsigned().Length + (ulong)SelectionEndIndex.WriteLEB128Unsigned().Length + 5;

            Save();
        }

        /// <summary>
        /// Writes content to a tab state file. Does not work on a 0.bin, 1.bin, or state file with unsaved buffer chunks. 
        /// </summary>
        /// <param name="newContent">Unicode Encoding</param>
        /// <returns>Bytes to be written to file</returns>
        public byte[] WriteContent(byte[] newContent)
        {
            ContentLength.Clear();
            ContentLength.Add((ulong)newContent.Length / 2);
            ContentLength.Add((ulong)newContent.Length / 2);
            ContentLength.Add((ulong)newContent.Length / 2);
            Content = newContent;
            Unsaved = [0x01];

            Save();

            return bytes;

            #region deprecated
            //using (MemoryStream stream = new MemoryStream(bytes))
            //{
            //    using (MemoryStream outStream = new MemoryStream())
            //    {
            //        using (BinaryReader reader = new BinaryReader(stream))
            //        {
            //            using (BinaryWriter writer = new BinaryWriter(outStream))
            //            {
            //                var hdr = reader.ReadBytes(2);
            //                writer.Write(hdr);

            //                writer.Write(reader.ReadLEB128Unsigned().WriteLEB128Unsigned());
            //                var typeFlag = reader.ReadLEB128Unsigned();
            //                writer.Write(typeFlag.WriteLEB128Unsigned());

            //                switch (typeFlag)
            //                {
            //                    case 0: //Unsaved - buffer file
            //                        {
            //                            CRC32Check c = new CRC32Check();
            //                            c.AddBytes(typeFlag);

            //                            var delim1 = reader.ReadBytes(1);
            //                            writer.Write(delim1);
            //                            c.AddBytes(delim1);

            //                            var OriginalContentLength = ContentLength.Last();
            //                            ContentLength.Clear();

            //                            reader.ReadLEB128Unsigned();
            //                            writer.Write(((ulong)newContent.Length / 2).WriteLEB128Unsigned());
            //                            ContentLength.Add((ulong)newContent.Length / 2);
            //                            c.AddBytes(ContentLength.Last());

            //                            reader.ReadLEB128Unsigned();
            //                            writer.Write(((ulong)newContent.Length / 2).WriteLEB128Unsigned());
            //                            ContentLength.Add((ulong)newContent.Length / 2);
            //                            c.AddBytes(ContentLength.Last());

            //                            var other = reader.ReadBytes(4); //Unknown maybe delimiter??? Appears to be WordWrap , Right to Left, Show Unicode, Unknown
            //                            writer.Write(other);
            //                            c.AddBytes(other);

            //                            reader.ReadLEB128Unsigned();
            //                            writer.Write(((ulong)newContent.Length / 2).WriteLEB128Unsigned());
            //                            ContentLength.Add((ulong)newContent.Length / 2);
            //                            c.AddBytes(ContentLength.Last());

            //                            reader.ReadBytes((int)OriginalContentLength * 2);
            //                            writer.Write(newContent);
            //                            Content = newContent;
            //                            c.AddBytes(Content);

            //                            var saved = reader.ReadBytes(1);
            //                            writer.Write(saved);
            //                            c.AddBytes(saved);

            //                            reader.ReadBytes(4);
            //                            CRC32Stored = c.CRC32;
            //                            writer.Write(CRC32Stored);
            //                            CRC32Calculated = c.CRC32;

            //                            //TODO: Delete or Update State Files? (0.bin and 1.bin)
            //                        }
            //                        break;
            //                    case 1: //Saved - buffer file
            //                        {
            //                            CRC32Check c = new CRC32Check();
            //                            c.AddBytes(typeFlag);

            //                            reader.ReadLEB128Unsigned();
            //                            writer.Write(FilePathLength.WriteLEB128Unsigned());
            //                            c.AddBytes(FilePathLength);

            //                            var fPathBytes = reader.ReadBytes((int)FilePathLength * 2);
            //                            writer.Write(fPathBytes);
            //                            c.AddBytes(fPathBytes);

            //                            reader.ReadLEB128Unsigned(); 
            //                            writer.Write(SavedFileContentLength.WriteLEB128Unsigned());
            //                            c.AddBytes(SavedFileContentLength);

            //                            reader.ReadBytes(1);
            //                            writer.Write(EncodingType);
            //                            c.AddBytes(EncodingType);

            //                            reader.ReadBytes(1);
            //                            writer.Write(CarriageReturnType);
            //                            c.AddBytes(CarriageReturnType);

            //                            var timeStamp = reader.ReadLEB128Unsigned();
            //                            writer.Write(timeStamp.WriteLEB128Unsigned());
            //                            c.AddBytes(timeStamp);

            //                            reader.ReadBytes(32);
            //                            writer.Write(FileHashStored);
            //                            c.AddBytes(FileHashStored);

            //                            var delim1 = reader.ReadBytes(2); //Unknown maybe delimiter??? Appears to be 00 01 
            //                            writer.Write(delim1);
            //                            c.AddBytes(delim1);

            //                            reader.ReadLEB128Unsigned();
            //                            writer.Write(SelectionStartIndex.WriteLEB128Unsigned());
            //                            c.AddBytes(SelectionStartIndex);
            //                            reader.ReadLEB128Unsigned();
            //                            writer.Write(SelectionEndIndex.WriteLEB128Unsigned());
            //                            c.AddBytes(SelectionEndIndex);


            //                            var delim2 = reader.ReadBytes(4); //Unknown maybe delimiter??? Appears to be WordWrap , Right to Left, Show Unicode, Unknown
            //                            writer.Write(delim2);
            //                            c.AddBytes(delim2);

            //                            var OriginalContentLength = ContentLength.Last();
            //                            ContentLength.Clear();

            //                            reader.ReadLEB128Unsigned();
            //                            writer.Write(((ulong)newContent.Length / 2).WriteLEB128Unsigned());
            //                            ContentLength.Add((ulong)newContent.Length / 2);
            //                            c.AddBytes(ContentLength.Last());

            //                            reader.ReadBytes((int)OriginalContentLength * 2);
            //                            writer.Write(newContent);
            //                            Content = newContent;
            //                            c.AddBytes(Content);

            //                            reader.ReadBytes(1); //Unsaved content flag This should be set to 1 or no? 
            //                            writer.Write(true);
            //                            Unsaved = BitConverter.GetBytes(true);
            //                            c.AddBytes(Unsaved);

            //                            reader.ReadBytes(4);
            //                            CRC32Stored = c.CRC32;
            //                            writer.Write(CRC32Stored);
            //                            CRC32Calculated = c.CRC32;

            //                            //TODO: Delete or Update State Files? (0.bin and 1.bin). May not matter
            //                        }
            //                        break;
            //                    default: //State File
            //                        {
            //                            throw new Exception("State File - Not supported");
            //                        }
            //                }


            //                #region Unsaved Buffer - There is no point in trying to edit these. Or is there???
            //                if (reader.BaseStream.Length > reader.BaseStream.Position)
            //                {
            //                    throw new Exception("Unsaved Buffer - Not supported");
            //                }
            //                #endregion

            //                bytes = outStream.ToArray();
            //            }

            //        }
            //    }
            //}
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public byte[] ChangeFilePath(string filePath, byte[] fileHashStored)
        {
            //TODO:
            FilePathLength = (ulong)filePath.Length;
            FilePath = filePath;
            FileHashStored = fileHashStored;

            Save();

            return bytes;
        }

        private void ParseBytes()
        {
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    string hdrType = Encoding.ASCII.GetString(reader.ReadBytes(2));

                    if (hdrType == "NP") //[0x4E, 0x50]
                    {
                        SequenceNumber = reader.ReadLEB128Unsigned();
                        TypeFlag = reader.ReadLEB128Unsigned();

                        switch (TypeFlag)
                        {
                            case 0: //Unsaved - buffer file
                                {
                                    CRC32Check c = new CRC32Check();
                                    c.AddBytes(TypeFlag);

                                    var un1 = reader.ReadBytes(1); //Unknown / Delimiter / 0x00
                                    c.AddBytes(un1);

                                    ContentLength.Add(reader.ReadLEB128Unsigned());
                                    c.AddBytes(ContentLength.Last());

                                    ContentLength.Add(reader.ReadLEB128Unsigned());
                                    c.AddBytes(ContentLength.Last());

                                    WordWrap = reader.ReadBytes(1);
                                    c.AddBytes(WordWrap);

                                    RightToLeft = reader.ReadBytes(1);
                                    c.AddBytes(RightToLeft);

                                    ShowUnicode = reader.ReadBytes(1);
                                    c.AddBytes(ShowUnicode);

                                    var un2 = reader.ReadBytes(1); //Unknown / Delimiter / 0x00
                                    c.AddBytes(un2);

                                    ContentLength.Add(reader.ReadLEB128Unsigned());
                                    c.AddBytes(ContentLength.Last());

                                    Content = reader.ReadBytes((int)ContentLength.Last() * 2);
                                    c.AddBytes(Content);
                                    
                                    Unsaved = reader.ReadBytes(1); 
                                    c.AddBytes(Unsaved);

                                    CRC32Stored = reader.ReadBytes(4);
                                    CRC32Calculated = c.CRC32;
                                }
                                break;
                            case 1: //Saved - buffer file
                                {
                                    CRC32Check c = new CRC32Check();
                                    c.AddBytes(TypeFlag);

                                    FilePathLength = reader.ReadLEB128Unsigned(); 
                                    c.AddBytes(FilePathLength);

                                    var fPathBytes = reader.ReadBytes((int)FilePathLength * 2);
                                    c.AddBytes(fPathBytes);

                                    FilePath = Encoding.Unicode.GetString(fPathBytes);

                                    SavedFileContentLength = reader.ReadLEB128Unsigned(); 
                                    c.AddBytes(SavedFileContentLength);

                                    EncodingType = reader.ReadBytes(1);
                                    c.AddBytes(EncodingType);

                                    CarriageReturnType = reader.ReadBytes(1);
                                    c.AddBytes(CarriageReturnType);

                                    var timeStamp = reader.ReadLEB128Unsigned();
                                    c.AddBytes(timeStamp);
                                    Timestamp = DateTime.FromFileTime((long)timeStamp);

                                    FileHashStored = reader.ReadBytes(32);
                                    c.AddBytes(FileHashStored);

                                    var delim1 = reader.ReadBytes(2); //Unknown / Delimiter / 0x00 0x01 //TODO: Maybe check for when this doesn't fit the assumed?
                                    c.AddBytes(delim1);

                                    SelectionStartIndex = reader.BaseStream.ReadLEB128Unsigned();
                                    c.AddBytes(SelectionStartIndex);
                                    SelectionEndIndex = reader.BaseStream.ReadLEB128Unsigned();
                                    c.AddBytes(SelectionEndIndex);

                                    WordWrap = reader.ReadBytes(1);
                                    c.AddBytes(WordWrap);

                                    RightToLeft = reader.ReadBytes(1);
                                    c.AddBytes(RightToLeft);

                                    ShowUnicode = reader.ReadBytes(1);
                                    c.AddBytes(ShowUnicode);

                                    var un2 = reader.ReadBytes(1); //Unknown / Delimiter / 0x00
                                    c.AddBytes(un2);

                                    ContentLength.Add(reader.BaseStream.ReadLEB128Unsigned());
                                    c.AddBytes(ContentLength.Last());
                                    Content = reader.ReadBytes((int)ContentLength.Last() * 2);
                                    c.AddBytes(Content);

                                    Unsaved = reader.ReadBytes(1); 
                                    c.AddBytes(Unsaved);

                                    CRC32Stored = reader.ReadBytes(4);
                                    CRC32Calculated = c.CRC32;
                                }
                                break;
                            default: //State File
                                {
                                    CRC32Check c = new CRC32Check();
                                    c.AddBytes(SequenceNumber);
                                    c.AddBytes(TypeFlag); 

                                    var un1 = reader.ReadBytes(1); //Unknown / Delimiter / 0x00
                                    c.AddBytes(un1);

                                    BinSize = reader.ReadLEB128Unsigned();
                                    c.AddBytes(BinSize);

                                    SelectionStartIndex = reader.ReadLEB128Unsigned();
                                    c.AddBytes(SelectionStartIndex);
                                    SelectionEndIndex = reader.ReadLEB128Unsigned();
                                    c.AddBytes(SelectionEndIndex);

                                    WordWrap = reader.ReadBytes(1);
                                    c.AddBytes(WordWrap);

                                    RightToLeft = reader.ReadBytes(1);
                                    c.AddBytes(RightToLeft);

                                    ShowUnicode = reader.ReadBytes(1);
                                    c.AddBytes(ShowUnicode);

                                    var un2 = reader.ReadBytes(1); //Unknown / Delimiter / 0x00
                                    c.AddBytes(un2);

                                    CRC32Stored = reader.ReadBytes(4);
                                    CRC32Calculated = c.CRC32;
                                }
                                break;
                        }


                        #region Unsaved Buffer
                        while (reader.BaseStream.Length > reader.BaseStream.Position)
                        {
                            CRC32Check c = new CRC32Check();

                            var charPos = reader.ReadLEB128Unsigned();
                            c.AddBytes(charPos);

                            var charDeletion = reader.ReadLEB128Unsigned();
                            c.AddBytes(charDeletion);

                            var charAddition = reader.ReadLEB128Unsigned();
                            c.AddBytes(charAddition);

                            var charAdded = reader.ReadBytes((int)charAddition * 2);

                            UnsavedBufferChunk chnk = new UnsavedBufferChunk(charPos, charDeletion, charAddition, charAdded, reader.ReadBytes(4), c.CRC32);
                            UnsavedBufferChunks.Add(chnk);
                        }
                        #endregion
                    }
                    else
                    {
                        throw new Exception("Invalid Header Bytes");
                    }
                }
            }
        }

        private void Save() //TODO: Expose this?
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(outStream))
                {
                    writer.Write([0x4E, 0x50]); 
                    writer.Write(SequenceNumber.WriteLEB128Unsigned()); 
                    writer.Write(TypeFlag.WriteLEB128Unsigned());

                    switch (TypeFlag)
                    {
                        case 0: //Unsaved File
                            {
                                CRC32Check c = new CRC32Check();
                                c.AddBytes(TypeFlag);

                                writer.Write(Unsaved);
                                c.AddBytes(Unsaved);

                                writer.Write(ContentLength.Last().WriteLEB128Unsigned());
                                c.AddBytes(ContentLength.Last());

                                writer.Write(ContentLength.Last().WriteLEB128Unsigned());
                                c.AddBytes(ContentLength.Last());

                                writer.Write(WordWrap);
                                c.AddBytes(WordWrap);

                                writer.Write(RightToLeft);
                                c.AddBytes(RightToLeft);

                                writer.Write(ShowUnicode);
                                c.AddBytes(ShowUnicode);

                                writer.Write([0x00]);
                                c.AddBytes([0x00]);

                                writer.Write(ContentLength.Last().WriteLEB128Unsigned());
                                c.AddBytes(ContentLength.Last());

                                writer.Write(Content);
                                c.AddBytes(Content);

                                writer.Write(Unsaved);
                                c.AddBytes(Unsaved);

                                CRC32Stored = c.CRC32;
                                CRC32Calculated = c.CRC32;
                                writer.Write(CRC32Stored);
                                break;
                            }
                        case 1: //Saved File
                            {
                                CRC32Check c = new CRC32Check();
                                c.AddBytes(TypeFlag);

                                writer.Write(FilePathLength.WriteLEB128Unsigned());
                                c.AddBytes(FilePathLength);

                                writer.Write(Encoding.Unicode.GetBytes(FilePath));
                                c.AddBytes(Encoding.Unicode.GetBytes(FilePath));

                                writer.Write(SavedFileContentLength.WriteLEB128Unsigned());
                                c.AddBytes(SavedFileContentLength);

                                writer.Write(EncodingType);
                                c.AddBytes(EncodingType);

                                writer.Write(CarriageReturnType);
                                c.AddBytes(CarriageReturnType);

                                writer.Write(((ulong)Timestamp.ToFileTime()).WriteLEB128Unsigned());
                                c.AddBytes((ulong)Timestamp.ToFileTime());

                                writer.Write(FileHashStored);
                                c.AddBytes(FileHashStored);

                                writer.Write([0x00, 0x01]);
                                c.AddBytes([0x00, 0x01]);

                                writer.Write(SelectionStartIndex.WriteLEB128Unsigned());
                                c.AddBytes(SelectionStartIndex);
                                writer.Write(SelectionEndIndex.WriteLEB128Unsigned());
                                c.AddBytes(SelectionEndIndex);

                                writer.Write(WordWrap);
                                c.AddBytes(WordWrap);

                                writer.Write(RightToLeft);
                                c.AddBytes(RightToLeft);

                                writer.Write(ShowUnicode);
                                c.AddBytes(ShowUnicode);

                                writer.Write([0x00]);
                                c.AddBytes([0x00]);

                                writer.Write(ContentLength.Last().WriteLEB128Unsigned());
                                c.AddBytes(ContentLength.Last());

                                writer.Write(Content);
                                c.AddBytes(Content);

                                writer.Write(Unsaved);
                                c.AddBytes(Unsaved);

                                CRC32Stored = c.CRC32;
                                CRC32Calculated = c.CRC32;
                                writer.Write(CRC32Stored);
                                break;
                            }
                        default: //State File 0.bin/1.bin
                            {
                                CRC32Check c = new CRC32Check();
                                c.AddBytes(SequenceNumber);
                                c.AddBytes(TypeFlag); 

                                writer.Write([0x00]);
                                c.AddBytes([0x00]);

                                writer.Write(BinSize.WriteLEB128Unsigned());
                                c.AddBytes(BinSize);

                                writer.Write(SelectionStartIndex.WriteLEB128Unsigned());
                                c.AddBytes(SelectionStartIndex);
                                writer.Write(SelectionEndIndex.WriteLEB128Unsigned());
                                c.AddBytes(SelectionEndIndex);

                                writer.Write(WordWrap);
                                c.AddBytes(WordWrap);

                                writer.Write(RightToLeft);
                                c.AddBytes(RightToLeft);

                                writer.Write(ShowUnicode);
                                c.AddBytes(ShowUnicode);

                                writer.Write([0x00]);
                                c.AddBytes([0x00]);

                                CRC32Stored = c.CRC32;
                                CRC32Calculated = c.CRC32;
                                writer.Write(CRC32Stored);
                                break;
                            }
                    }

                    foreach (UnsavedBufferChunk chnk in UnsavedBufferChunks)
                    {
                        writer.Write(chnk.CursorPosition.WriteLEB128Unsigned());
                        writer.Write(chnk.DeletionAction.WriteLEB128Unsigned());
                        writer.Write(chnk.AdditionAction.WriteLEB128Unsigned());
                        if (chnk.AdditionAction > 0)
                        {
                            writer.Write(chnk.CharactersAdded); //TODO: Verify this
                        }
                        writer.Write(chnk.CRC32Stored);
                    }

                    bytes = outStream.ToArray();
                }
            }
        }
    }
}
