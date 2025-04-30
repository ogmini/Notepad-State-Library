using NotepadStateLibrary;


string tabStateLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Packages\Microsoft.WindowsNotepad_8wekyb3d8bbwe\LocalState\TabState");
string windowStateLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Packages\Microsoft.WindowsNotepad_8wekyb3d8bbwe\LocalState\WindowState");

foreach (var path in Directory.EnumerateFiles(tabStateLocation, "*.bin"))
{
    using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
    {
        byte[] data = new byte[fileStream.Length];
        fileStream.Read(data);

        if (data.Length > 0)
        {
            Console.WriteLine("Processing TabState - {0}", Path.GetFileName(path));
            NPTabState np = new NPTabState(data, Path.GetFileName(path));

            switch (np.TypeFlag)
            {
                case 0:
                    //No File
                    //Validate file against expected
                    bool passNoFile = np.ContentLength == 56
                        && np.ContentString == "Lorem ipsum dolor sit amet, consectetur adipiscing elit."
                        && np.OptionCount == 2
                        && np.SelectionStartIndex == np.SelectionEndIndex
                        && np.SelectionStartIndex == 56
                        && np.bytes.Length == 131;    
                    if (!passNoFile)
                    {
                        throw new Exception("No File Failure");
                    }
                    break;
                case 1:
                    //File
                    //Validate file against expected
                    bool passFile = np.ContentLength == 1653
                        && np.FilePath.EndsWith("SavedFile.txt")
                        && np.OptionCount == 2
                        && np.SavedFileContentLength == 1599
                        && np.SelectionStartIndex == np.SelectionEndIndex
                        && np.SelectionStartIndex == 56
                        && (np.bytes.Length == 3483 || np.bytes.Length == 3528);

                    if (!passFile)
                    {
                        throw new Exception("File Failure");
                    }
                    break;
                default:
                    //State
                    //TODO: Validate file against expected
                    break;
            }

            if (np.UnsavedBufferChunks.Count > 0)
            {
                //Validate file against known
                bool passChunks = np.UnsavedBufferChunks.Count == 5
                    && np.UnsavedBufferChunks[0].AdditionAction == 1
                    && np.UnsavedBufferChunks[0].CursorPosition == 56
                    && np.UnsavedBufferChunks[0].CharactersAddedString == "A"
                    && np.UnsavedBufferChunks[0].DeletionAction == 0
                    && np.UnsavedBufferChunks[0].CRC32Stored.SequenceEqual(np.UnsavedBufferChunks[0].CRC32Calculated);

                if (!passChunks)
                {
                    throw new Exception("Chunk Failure");
                }
            }

            if (!np.CRC32Calculated.SequenceEqual(np.CRC32Stored))
            {
                throw new Exception("CRC32 Failure");
            }
        }
    }
}

foreach (var path in Directory.EnumerateFiles(windowStateLocation, "*.bin"))
{
    using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
    {
        byte[] data = new byte[fileStream.Length];
        fileStream.Read(data);

        if (data.Length > 0)
        {
            Console.WriteLine("Processing WindowState - {0}", Path.GetFileName(path));
            NPWindowState np = new NPWindowState(data, Path.GetFileName(path));

            //TODO: Validate file against expected
            bool passWindow = np.BytesToCRC == 44 || np.BytesToCRC == 60;

            if (!passWindow)
            {
                throw new Exception("Window State Failure");
            }

            if (!np.CRC32Calculated.SequenceEqual(np.CRC32Stored))
            {
                throw new Exception("CRC32 Failure");
            }
        }
    }
}