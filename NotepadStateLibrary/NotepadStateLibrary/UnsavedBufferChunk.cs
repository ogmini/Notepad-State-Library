using System.Text;

namespace NotepadStateLibrary
{
    public struct UnsavedBufferChunk
    {
        /// <summary>
        /// 
        /// </summary>
        public string FileName { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public ulong CursorPosition {  get; }
        /// <summary>
        /// 
        /// </summary>
        public ulong DeletionAction { get; }
        /// <summary>
        /// 
        /// </summary>
        public ulong AdditionAction { get; }
        /// <summary>
        /// 
        /// </summary>
        public byte[]? CharactersAdded { get; }
        /// <summary>
        /// 
        /// </summary>
        public string CharactersAddedString { get { return CharactersAdded == null ? string.Empty : Encoding.Unicode.GetString(CharactersAdded).ReplaceLineEndings(); } }
        /// <summary>
        /// 
        /// </summary>
        public byte[] CRC32Stored { get; }
        /// <summary>
        /// 
        /// </summary>
        public byte[] CRC32Calculated { get; }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="CursorPosition"></param>
        /// <param name="DeletionAction"></param>
        /// <param name="AdditionAction"></param>
        /// <param name="CharactersAdded"></param>
        /// <param name="CRC32Stored"></param>
        /// <param name="CRC32Calculated"></param>
        public UnsavedBufferChunk(string FileName, ulong CursorPosition, ulong DeletionAction, ulong AdditionAction, byte[]? CharactersAdded, byte[] CRC32Stored, byte[] CRC32Calculated) 
        {
            this.FileName = FileName; 
            this.CursorPosition = CursorPosition;
            this.DeletionAction = DeletionAction;
            this.AdditionAction = AdditionAction;
            this.CharactersAdded = CharactersAdded;
            this.CRC32Stored = CRC32Stored;
            this.CRC32Calculated = CRC32Calculated;
        }
    }
}
