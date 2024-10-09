using System.Reflection.PortableExecutable;
using System.Text;


namespace NotepadStateLibrary
{
    public class NPTabState
    {
        //TODO: Reorder these to a sensical order?

        /// <summary>
        /// 
        /// </summary>
        public string FileName { get; private set; }
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
        public ulong ContentLength { get; private set; }
        /// <summary>
        /// Calculated CRC32
        /// </summary>
        public string ContentString
        {
            get { return Content == null ? string.Empty : Encoding.Unicode.GetString(Content).ReplaceLineEndings(); } 
        }
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
        public ulong OptionCount { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public byte[] Options { get; private set; } = null!;
        /// <summary>
        /// Right To Left Text Toggle
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
        /// Show Unicode Toggle
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
        /// Unsaved Toggle
        /// </summary>
        public byte[] Unsaved { get; private set; } = null!;
        /// <summary>
        /// Buffer of unsaved changes.
        /// </summary>
        public List<UnsavedBufferChunk> UnsavedBufferChunks { get; private set; }
        /// <summary>
        /// WordWrap Toggle
        /// </summary>
        public byte[] WordWrap { get; private set; } = null!;

        /// <summary>
        /// Notepad Tab State Files
        /// </summary>
        /// <param name="bytes">Bytes of the state file</param>
        public NPTabState(byte[] bytes, string fileName)
        {
            this.bytes = bytes;
            this.FileName = fileName;

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
            UnsavedBufferChunks = new List<UnsavedBufferChunk>();

            Content = content;
            ContentLength = (ulong)content.Length / 2;
            SelectionStartIndex = ContentLength;
            SelectionEndIndex = ContentLength;
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
            ContentLength = (ulong)content.Length / 2; 
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
            ContentLength = ((ulong)newContent.Length / 2);
            Content = newContent;
            Unsaved = [0x01];

            Save();

            return bytes;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public byte[] WriteFilePath(string filePath, byte[] fileHashStored)
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

                                    OptionCount = reader.ReadLEB128Unsigned();
                                    c.AddBytes(OptionCount);

                                    //Read Extra Options
                                    Options = reader.ReadBytes((int)OptionCount);
                                    c.AddBytes(Options);

                                    ContentLength = reader.ReadLEB128Unsigned();
                                    c.AddBytes(ContentLength);

                                    Content = reader.ReadBytes((int)ContentLength * 2);
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

                                    OptionCount = reader.ReadLEB128Unsigned();
                                    c.AddBytes(OptionCount);

                                    //Read Extra Options
                                    Options = reader.ReadBytes((int)OptionCount);
                                    c.AddBytes(Options);

                                    ContentLength = reader.ReadLEB128Unsigned();
                                    c.AddBytes(ContentLength);
                                    Content = reader.ReadBytes((int)ContentLength * 2);
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

                                    OptionCount = reader.ReadLEB128Unsigned();
                                    c.AddBytes(OptionCount);

                                    //Read Extra Options
                                    Options = reader.ReadBytes((int)OptionCount);
                                    c.AddBytes(Options);

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

                                writer.Write(OptionCount);
                                c.AddBytes(OptionCount);

                                //Write Extra Options
                                if (OptionCount > 0)
                                {
                                    writer.Write(Options);
                                    c.AddBytes(Options);
                                }

                                writer.Write(ContentLength.WriteLEB128Unsigned());
                                c.AddBytes(ContentLength);

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

                                writer.Write(OptionCount);
                                c.AddBytes(OptionCount);

                                //Write Extra Options
                                if (OptionCount > 0)
                                {
                                    writer.Write(Options);
                                    c.AddBytes(Options);
                                }

                                writer.Write(ContentLength.WriteLEB128Unsigned());
                                c.AddBytes(ContentLength);

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

                                writer.Write(OptionCount);
                                c.AddBytes(OptionCount);

                                //Write Extra Options
                                if (OptionCount > 0)
                                {
                                    writer.Write(Options);
                                    c.AddBytes(Options);
                                }

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
