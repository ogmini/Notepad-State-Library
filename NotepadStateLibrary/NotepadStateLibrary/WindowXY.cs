namespace NotepadStateLibrary
{
    public struct WindowXY
    {
        /// <summary>
        /// 
        /// </summary>
        public int X { get; } 
        /// <summary>
        /// 
        /// </summary>
        public int Y { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public WindowXY(int x, int y) 
        {
            X = x;
            Y = y;
        }
    }
}
