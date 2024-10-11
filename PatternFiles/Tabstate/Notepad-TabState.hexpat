#pragma author ogmini https://github.com/ogmini JustArion https://github.com/JustArion
#pragma description Windows 11 Notepad Tab State file
// File found at %localappdata%\Packages\Microsoft.WindowsNotepad_8wekyb3d8bbwe\LocalState\TabState
#pragma magic [ 4E 50 ??] @ 0x00

#include <std/string.pat>
#include <type/leb128.pat>
#include <std/time.pat>
#include <std/core.pat>

using ul = type::uLEB128;
using int = u32;
using byte = u8;


// https://github.com/ogmini/Notepad-Tabstate-Buffer/blob/main/README.md#insertion-chunk
struct UnsavedChunk
{
    ul CursorPosition;
    ul CharactersDeleted;
    ul CharactersAdded;
    char16 AddedCharacters[CharactersAdded];
    byte CRC32[4]  [[format("format_to_hex")]];
};

// --- Data Types ---

// Default fallback is UTF-8
enum EncodingTypes : byte
{
    UTF8 = 5,
    UTF8BOM = 4,
    UTF16BE = 3,
    UTF16LE = 2,
    ANSI = 1,
};

enum EOLSequenceType : byte
{
    Windows_CRLF = 1, // Windows
    Mac_CR = 2, // Mac
    Unix_LF = 3 // Linux
};

enum TypeFlag : byte
{
	Unsaved = 0,
    Saved = 1,
    NoFileTab = 10,
	FileTab = 11
};

struct Header
{
	char Magic_Bytes[2]; // NP
    // https://github.com/Nordgaren/tabstate-util/issues/1#issuecomment-1988731585
    ul Sequence_Number; // 0 in Saved files.
    TypeFlag Type_Flag;
};

struct SelectionInfo
{
    ul SelectionStartIndex;
    ul SelectionEndIndex;
};

// https://github.com/Nordgaren/tabstate-util/blob/master/TabState.bt#L48
struct TabOptions
{
    byte WordWrap;
    byte RightToLeft;
    byte ShowUnicode;
    ul MoreOptions;
    byte unknown_options[MoreOptions];
};

struct SavedFileInfo
{
    ul FilePathLength;
    char16 SavedFilePath[FilePathLength];
    ul ContentLength;
    EncodingTypes EncodingType;
    EOLSequenceType CarriageReturnType;
    //ul FileTime [[comment("18-digit Win32 FILETIME")]];
    ul Timestamp [[format("filetime_to_local"),comment("18-digit Win32 FILETIME")]] ;
    byte FileHash[32] [[format("format_to_hex")]];
};

struct NotepadTab
{
    Header HeaderInfo;

    if (HeaderInfo.Type_Flag == 0) 
    {
        byte unk [[comment("Appears to always be 0x1")]]; 
    }
    else if (HeaderInfo.Type_Flag == 1)
    {
        SavedFileInfo FileInfo;
        
        byte unk[2] [[comment("Appears to always be 0x0 0x1")]]; 
    }
    else
    {
        //state file
        byte unk;
		ul BinSize [[comment("Size in bytes of the associated *.bin")]];
    }
    
    SelectionInfo CursorPosition;
    TabOptions ConfigurationBlock;
    
    if (HeaderInfo.Type_Flag == 0)
    {
    ul ContentLength;
    char16 Content[ContentLength];
    bool Unsaved; 
    }
    else if (HeaderInfo.Type_Flag == 1)
    {
    ul ContentLength;
    char16 Content[ContentLength];
    bool Unsaved; 
    }
    else
    {
    }
        
    byte CRC32[4]  [[format("format_to_hex"),comment("CRC32 of .bin")]];
    
    //
    UnsavedChunk Chunks[while(!std::mem::eof())];
};

// --- Functions ---



fn filetime_to_local(ul time)
{
    int epoch = std::time::filetime_to_unix(time);
    return std::time::format(std::time::to_local(epoch), "%Y-%m-%d | %H:%M:%S");
};

fn format_to_hex(auto data)
{
    int length = std::core::member_count(data);
    str result;
    for(int i = 0, i < length, i = i+1)
    {
        result = result + std::format("{:X}", u8(data[i]));
    }
    return result;
};

// -- Declaration --

NotepadTab TabState @ 0x0;

// -- Presentation --

if (TabState.HeaderInfo.Type_Flag == 0)
{
    str formattedFileTime = filetime_to_local(TabState.FileInfo.Timestamp);

    std::print("Cursor Position: {0}", TabState.CursorPosition.SelectionStartIndex);

    std::print("ConfigurationBlock{0}", TabState.ConfigurationBlock);
    
    if (TabState.CursorPosition.SelectionStartIndex != TabState.CursorPosition.SelectionEndIndex)
    {
        std::print("Selection End Index: {0}", TabState.CursorInfo.SelectionEndIndex);
        str slice = std::string::substr(std::string::to_string(TabState.Content), TabState.CursorInfo.SelectionStartIndex, TabState.CursorInfo.SelectionEndIndex - TabState.CursorInfo.SelectionStartIndex);
        std::print("Selected Text: {0}", slice);
    }
	
	std::print("Content: {0}", TabState.Content);
		
	// -- Output any unsaved buffer chunks --
	std::print("\nUnsaved Buffer Chunks\n------");

	std::print("Chunks:\n{0}", TabState.Chunks);

	// -- Print Unknowns --
	std::print("\nUnknowns\n------");

	std::print("unk: {0}", TabState.unk);
	std::print("Unknown ConfigurationBlockOption: {0}\n", TabState.ConfigurationBlock.MoreOptions);
}
else if (TabState.HeaderInfo.Type_Flag == 1)
{
	str formattedFileTime = filetime_to_local(TabState.FileInfo.Timestamp);

    std::print("Encoding Type: {0}", std::string::to_string(TabState.FileInfo.EncodingType));
    std::print("Line Carriage Type: {0}", std::string::to_string(TabState.FileInfo.CarriageReturnType));
    std::print("File: {0}", TabState.FileInfo.SavedFilePath);
    std::print("Saved at " + formattedFileTime);
    std::print("FileHash{0}", format_to_hex(TabState.FileInfo.FileHash));

    std::print("Cursor Position: {0}", TabState.CursorPosition.SelectionStartIndex);

    std::print("ConfigurationBlock{0}", TabState.ConfigurationBlock);
    
    if (TabState.CursorPosition.SelectionStartIndex != TabState.CursorPosition.SelectionEndIndex)
    {
        std::print("Selection End Index: {0}", TabState.CursorInfo.SelectionEndIndex);
        str slice = std::string::substr(std::string::to_string(TabState.Content), TabState.CursorInfo.SelectionStartIndex, TabState.CursorInfo.SelectionEndIndex - TabState.CursorInfo.SelectionStartIndex);
        std::print("Selected Text: {0}", slice);
    }
	
	std::print("Content: {0}", TabState.Content);
	
	
	// -- Output any unsaved buffer chunks --
	std::print("\nUnsaved Buffer Chunks\n------");

	std::print("Chunks:\n{0}", TabState.Chunks);

	// -- Print Unknowns --
	std::print("\nUnknowns\n------");

	std::print("unk: {0}", TabState.unk);
	std::print("Unknown ConfigurationBlockOption: {0}\n", TabState.ConfigurationBlock.MoreOptions);
}
else
{

}


std::print("CRC32: {0}", format_to_hex(TabState.CRC32));
