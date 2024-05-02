# Notepad State Library

Microsoft Windows 11's version of [Windows Notepad](https://apps.microsoft.com/detail/9msmlrh6lzf3) supports multiple tabs and many other features. This repository serves to record and document my research and efforts in reverse engineering the format of the tabstate and windowstate files and understanding their behavior. The result of which is a C# library that provides the ability to parse and manipulate the tabstate and windowstate files. Additionally, the following are planned/completed:
- [x] Tabstate Parser
- [x] Windowstate Parser
- [x] 010 Editor Binary Template File for Windowstate file
- [x] ImHex Pattern File for Windowstate file 
- [ ] Tabstate Manipulator
- [ ] Windowstate Manipulator
- [ ] POC Malware

This library and its tools could be useful in forensic investigations or even in the toolbox for a red/purple team.

## Acknowledgements

[jlogsdon](https://github.com/jlogsdon) for lots of help.  
[NordGaren](https://github.com/Nordgaren) for lots of help, his [tabstate-util](https://github.com/Nordgaren/tabstate-util/), and 010 Editor Binary Template file which helped me learn the syntax and the struct/functions for uLEB128.   
[JustArion](https://github.com/JustArion) for lots of help and his [ImHex Pattern File](https://github.com/JustArion/Notepad-Tabs/blob/main/ImHex-Patterns/NotepadTab.hexpat) which helped me learn the syntax.

## Usage
> [!WARNING]
> Prior to using the library or any of the tools, you should have an understanding of the tabstate and windowstate files. 
>
> [Information Section](#information)



### Library
Documentation WIP
### Pattern Files
Binary Template for 010 Editor and Pattern File for ImHex have been submitted to their respective repositories and should be available for use. 
### Tabstate Parser
Documentation WIP
### Windowstate Parser
Documentation WIP

## Information

### Tabstate 

> [!NOTE]
> Location of Files
> `%localappdata%\Packages\Microsoft.WindowsNotepad_8wekyb3d8bbwe\LocalState\TabState`

The tabstate files store information about the open tabs and their contents in Windows Notepad. The filenames are GUIDs and there are three types of *.bin files:
- File Tab
	- These tabs have been saved to disk or have been opened from a file on disk 
- No File Tab
	- These tabs have not been saved to disk and have not been opened from a file on disk. They only exist in the buffer 
- State File
	- These are the *.0.bin and *.1.bin files and store option information about the related matching GUID *.bin

Both the File and No File Tab can have related State Files. 

While Windows Notepad is open the File and No File Tab can have [Unsaved Buffer Chunks](#unsaved-buffer-chunk) of changes that haven't been saved or consolidated. The [Unsaved Buffer Chunks](#unsaved-buffer-chunk) can be used to playback the changes to the text similar to a keylogger. Once Windows Notepad is closed, the [Unsaved Buffer Chunks](#unsaved-buffer-chunk) are consolidated into the content. 

#### Behavior

Opening Windows Notepad with no currently existing tab(s) will create an empty "Untitled" tab and an associated No File Tab bin file.

Creating new tab(s) will create associated No File Tab bin file(s).

Opening file(s) from disk will create associated File Tab bin file(s).

Closing tab(s) will delete the associated bin file(s). 

TODO: Reasons State Files are created/deleted.

The existence of no bin file(s) indicates:
- Windows Notepad has never been opened
- All tabs have been manually closed
- Manual deletion

If you drag/drop multiple files into Windows Notepad, the internal content of the bin file(s) will not load until the tab becomes active. (To be expaned upon)

#### File Format

##### File Tab
- Signature / Magic Bytes [0x4E, 0x50] "NP" 
- Sequence Number (uLEB128)
- TypeFlag (uLEB128)
- FilePathLength (uLEB128)
- FilePath (Variable)
- SavedFileContentLength (uLEB128)
- EncodingType (1 byte) 
- CarriageReturnType (1 byte) 
- Timestamp (uLEB128)
- FileHash (32 bytes)
- :question:Unknown [0x00, 0x01]
- SelectionStartIndex (uLEB128)
- SelectionEndIndex (uLEB128)
- [Configuration Block](#configuration-block)
- ContentLength (uLEB128)
- Content (Variable)
- Unsaved (1 byte) 
- CRC32 (4 bytes)
- [Unsaved Buffer Chunks](#unsaved-buffer-chunk)

##### No File Tab
- Signature / Magic Bytes [0x4E, 0x50] "NP" 
- Sequence Number (uLEB128)
- TypeFlag (uLEB128)
- :question:Unknown [0x01]
- SelectionStartIndex (uLEB128) - Never differs from ContentLength
- SelectionEndIndex (uLEB128) - Never differs from ContentLength
- [Configuration Block](#configuration-block)
- ContentLength (uLEB128)
- Content (Variable)
- Unsaved (1 byte)
- CRC32 (4 bytes)
- [Unsaved Buffer Chunks](#unsaved-buffer-chunk)

##### State File
- Signature / Magic Bytes [0x4E, 0x50] "NP" 
- Sequence Number (uLEB128)
- TypeFlag (uLEB128)
- :question:Unknown [0x00]
- BinSize (uLEB128)
- SelectionStartIndex (uLEB128)
- SelectionEndIndex (uLEB128)
- [Configuration Block](#configuration-block)
- CRC32 (4 bytes)

###### Configuration Block
- WordWrap (1 byte) 
- RightToLeft (1 byte) 
- ShowUnicode (1 byte) 
- Version/MoreOptions (uLEB128)
- [More Options Block](#more-options-block) (Variable length based on Version/MoreOptions)

###### More Options Block
- SpellCheck (Not verified)
- Autocorrect (Not verified)

###### Unsaved Buffer Chunk
- Cursor Position (uLEB128)
- Delection Action (uLEB128)
- Addition Action (uLEB128)
- Added Characters (UTF-16LE)
- CRC32 (4 bytes)

### Windowstate 

> [!NOTE]
> Location of Files
> `%localappdata%\Packages\Microsoft.WindowsNotepad_8wekyb3d8bbwe\LocalState\WindowState`

The windowstate files store information about the list of tabs, order of tabs, and active tab for Windows Notepad. Tabs are stored as GUIDs which refer back to the filename of the matching tabstate file. They also store the coordinates and size of the Windows Notepad window. Integrity of the file is validated with CRC32. 

#### Behavior

Adding a tab adds another Tab GUID Chunk to the collection of Chunks and updates the number of bytes to the CRC32. Any existing slack space in the file will get overwritten up to the end of the new CRC32.

Closing a tab deletes the relevant Tab GUID Chunk from the collection of Chunks and updates the number of bytes to the CRC32. Slack space after the CRC32 may result from closing tabs. The files appear to never get smaller. More testing is required to validate this assumption.

The following actions will cause an update of the sequence number and file:
- Resizing window
- Moving window
- Reordering/moving tabs
- Closing tab(s)
	- Closing multiple tabs at once results in one action
- Opening tab(s)

Creating a new Windows Notepad window by dragging a tab outside of the original window will spawn new window state files. As you close each extra window, it will prompt you to save any files in that window and the corresponding window state file pair will be deleted. When the last window of Windows Notepad is closed, the final window state file pair will not be deleted. 

Updates alternate between the *.0.bin and *.1.bin with the most up to date file having the greatest sequence number.

#### File Format
- Signature / Magic Bytes [0x4E, 0x50] "NP" 
- Sequence Number (uLEB128)
- BytesToCRC (uLEB128)
- :question:Unknown [0x00]
- NumberTabs (uLEB128)
- Tab GUID Chunks
	- GUID for each Tab in view order (16 bytes) 
	- These GUIDs refer to the filename of the matching tabstate file
- ActiveTab (uLEB128)
- TopLeftCoords 
	- X (uINT32)
	- Y uINT32)
- BottomRightCoords 
	- X (uINT32)
	- Y uINT32)
- WindowSize 
	- Width (uINT32)
	- Height (uINT32)
- :question:Unknown [0x00]
- CRC32 (4 bytes)
- Slack Space (Variable)

#### Slack Space
It appears that the windowstate files will never reduce in size. More testing is required to validate this or to discover what actions will cause them to be deleted or cleared out.

There is a potential to recover complete or partial GUIDs from the slack space that can be tied back to past tabstate files. These deleted tabstate files could possibly be recovered and examined.  

> [!WARNING]  
> The approaches make heavy assumptions. As Tabs are opened and closed, the slack space will get more and more convoluted and disarrayed. Manual parsing is suggested and there is no guarantee of being able to recover anything of use. 

##### Approaches

WIP