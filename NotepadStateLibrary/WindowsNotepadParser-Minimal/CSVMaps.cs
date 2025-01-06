using NotepadStateLibrary;
using CsvHelper.Configuration;

namespace WindowsNotepadParser_Minimal
{
    public sealed class NoFileMap : ClassMap<NPTabState>
    {
        public NoFileMap() 
        { 
            AutoMap(System.Globalization.CultureInfo.InvariantCulture);
            Map(m => m.FilePathLength).Ignore();
            Map(m => m.FilePath).Ignore();
            Map(m => m.SavedFileContentLength).Ignore();
            Map(m => m.EncodingType).Ignore();
            Map(m => m.CarriageReturnType).Ignore();
            Map(m => m.Timestamp).Ignore();
            Map(m => m.FileHashStored).Ignore();
            Map(m => m.BinSize).Ignore();

            Map(m => m.FileName).Index(0);
            Map(m => m.SequenceNumber).Index(1);
            Map(m => m.TypeFlag).Index(2);
            Map(m => m.SelectionStartIndex).Index(3);
            Map(m => m.SelectionEndIndex).Index(4);
            Map(m => m.WordWrap).Index(5);
            Map(m => m.RightToLeft).Index(6);
            Map(m => m.ShowUnicode).Index(7);
            Map(m => m.OptionCount).Index(8);
            Map(m => m.Options).Index(9);
            Map(m => m.ContentLength).Index(10);
            Map(m => m.Content).Index(11);
            Map(m => m.ContentString).Index(12);
            Map(m => m.Unsaved).Index(13);
            Map(m => m.CRC32Stored).Index(14);
            Map(m => m.CRC32Calculated).Index(15);
            Map(m => m.bytes).Index(16);
        }
    }

    public sealed class FileMap : ClassMap<NPTabState> 
    { 
        public FileMap()
        {
            AutoMap(System.Globalization.CultureInfo.InvariantCulture);

            Map(m => m.BinSize).Ignore();

            Map(m => m.FileName).Index(0);
            Map(m => m.SequenceNumber).Index(1);
            Map(m => m.TypeFlag).Index(2);
            Map(m => m.FilePathLength).Index(3);
            Map(m => m.FilePath).Index(4);
            Map(m => m.SavedFileContentLength).Index(5);
            Map(m => m.EncodingType).Index(6);
            Map(m => m.CarriageReturnType).Index(7);
            Map(m => m.Timestamp).Index(8);
            Map(m => m.FileHashStored).Index(9);
            Map(m => m.SelectionStartIndex).Index(10);
            Map(m => m.SelectionEndIndex).Index(11);
            Map(m => m.WordWrap).Index(12);
            Map(m => m.RightToLeft).Index(13);
            Map(m => m.ShowUnicode).Index(14);
            Map(m => m.OptionCount).Index(15);
            Map(m => m.Options).Index(16);
            Map(m => m.ContentLength).Index(17);
            Map(m => m.Content).Index(18);
            Map(m => m.ContentString).Index(19);
            Map(m => m.Unsaved).Index(20);
            Map(m => m.CRC32Stored).Index(21);
            Map(m => m.CRC32Calculated).Index(22);
            Map(m => m.bytes).Index(23);
        }
    }

    public sealed class StateMap : ClassMap<NPTabState>
    {
        public StateMap() 
        {
            AutoMap(System.Globalization.CultureInfo.InvariantCulture);
            Map(m => m.FilePathLength).Ignore();
            Map(m => m.FilePath).Ignore();
            Map(m => m.SavedFileContentLength).Ignore();
            Map(m => m.EncodingType).Ignore();
            Map(m => m.CarriageReturnType).Ignore();
            Map(m => m.Timestamp).Ignore();
            Map(m => m.FileHashStored).Ignore();
            Map(m => m.ContentLength).Ignore();
            Map(m => m.Content).Ignore();
            Map(m => m.ContentString).Ignore();
            Map(m => m.Unsaved).Ignore();

            Map(m => m.FileName).Index(0);
            Map(m => m.SequenceNumber).Index(1);
            Map(m => m.TypeFlag).Index(2);
            Map(m => m.BinSize).Index(3);
            Map(m => m.SelectionStartIndex).Index(4);
            Map(m => m.SelectionEndIndex).Index(5);
            Map(m => m.WordWrap).Index(6);
            Map(m => m.RightToLeft).Index(7);
            Map(m => m.ShowUnicode).Index(8);
            Map(m => m.OptionCount).Index(9);
            Map(m => m.Options).Index(10);
            Map(m => m.CRC32Stored).Index(11);
            Map(m => m.CRC32Calculated).Index(12);
            Map(m => m.bytes).Index(13);
        }
    }

    public sealed class WindowMap : ClassMap<NPWindowState>
    {
        public WindowMap() 
        {
            AutoMap(System.Globalization.CultureInfo.InvariantCulture);
            Map(m => m.FileName).Index(0);
            Map(m => m.SequenceNumber).Index(1);
            Map(m => m.BytesToCRC).Index(2);
            Map(m => m.NumberTabs).Index(3);
            Map(m => m.TabsList).Index(4);
            Map(m => m.ActiveTab).Index(5);
            Map(m => m.TopLeftCoords).Index(6);
            Map(m => m.BottomRightCoords).Index(7);
            Map(m => m.WindowSize).Index(8);
            Map(m => m.CRC32Stored).Index(9);
            Map(m => m.CRC32Calculated).Index(10);
            Map(m => m.bytes).Index(11);
        }
    }
}
