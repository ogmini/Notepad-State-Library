# Notepad State Library

Microsoft Windows 11's version of [Windows Notepad](https://apps.microsoft.com/detail/9msmlrh6lzf3) supports multiple tabs and many other features. This repository serves to record and document my research and efforts in reverse engineering the format of the tabstate and windowstate files and understand their behavior. The result of which is a C# library and toolset that provides the ability to parse and manipulate the tabstate, windowstate, and settings files. 

The following are planned/completed:
- [x] Tabstate Parser
- [x] Windowstate Parser
- [x] 010 Editor Binary Template File for Windowstate/Tabstate file
- [x] ImHex Pattern File for Windowstate/Tabstate file 
- [ ] Tabstate Manipulator
- [ ] Windowstate Manipulator
- [ ] Settings.dat / Application Registry
- [ ] Ways to detect manipulation
- [ ] POC Malware

This library and its tools could be useful in forensic investigations or even in the toolbox for a red/purple team.

## Usage
> [!WARNING]
> Prior to using the library or any of the tools, you should have an understanding of the tabstate, windowstate, and settings files. 
>
> [Information Section](#information)
> - [Tabstate](#tabstate)
> - [Windowstate](#windowstate)
> - [Settings](#settings)

### Library
Documentation WIP
### Pattern Files
Binary Templates for 010 Editor and Pattern Files for ImHex have been submitted to their respective repositories and should be available for use. They can also be found in this repository at [https://github.com/ogmini/Notepad-State-Library/tree/main/PatternFiles](https://github.com/ogmini/Notepad-State-Library/tree/main/PatternFiles).
### Parser
Running `WindowsNotepadParser.exe` will check the default locations for [Tabstate](#tabstate) and [Windowstate](#windowstate) files and generate CSV files which can be viewed in tools such as [Timeline Explorer](https://ericzimmerman.github.io/) or Excel. GIFs will also be generated for any detected [Unsaved Buffer Chunks](#unsaved-buffer-chunk) to visualize the changes of the content over time.  

Running `WindowsNotepadParser.exe --help` will display flag options.

## Information

The information below has been tested/validated on the following configurations:

| Windows Build | Windows Notepad Version 
|---|---|
| Windows 11 23H2 OS Build 22635.3566 (Beta Release Branch) | 11.2402.22.0
| Windows 11 23H2 OS Build 22631.3527 (Stable Release Branch) | 11.2402.22.0
| Windows 11 23H2 OS Build 22631.3737 (Stable Release Branch) | 11.2404.10.0
| Windows 11 23H2 OS Build 22631.4317 (Stable Release Branch) | 11.2407.9.0
| Windows 11 23H2 OS Build 22631.4317 (Stable Release Branch) | 11.2408.12.0


### Tabstate 

> [!NOTE]
> Location of Files
> `%localappdata%\Packages\Microsoft.WindowsNotepad_8wekyb3d8bbwe\LocalState\TabState`
>
> Relevant Files
> `*.bin` `*.0.bin` `*.1.bin`

The tabstate files store information about the open tabs and their contents in Windows Notepad. The filenames are GUIDs and there are three types of *.bin files:
- _File Tab_
	- These tabs have been saved to disk or have been opened from a file on disk. 
	- They have a TypeFlag of 1. 
- _No File Tab_
	- These tabs have not been saved to disk and have not been opened from a file on disk. They only exist in the *.bin files. 
	- They have a TypeFlag of 0.
- _State File_
	- These are the *.0.bin and *.1.bin files and store extra information about the related matching GUID *.bin. 
	- They have a TypeFlag greater than 1.

Both the _File Tab_ and _No File Tab_ can have related _State Files_. 

While Windows Notepad is open the _File Tab_ and _No File Tab_ can have [Unsaved Buffer Chunks](#unsaved-buffer-chunk) of changes that haven't been flushed. The [Unsaved Buffer Chunks](#unsaved-buffer-chunk) can be used to playback the changes to the text similar to a keylogger. Once Windows Notepad is closed, the [Unsaved Buffer Chunks](#unsaved-buffer-chunk) are flushed into the content. 

Integrity of the file is validated with CRC32. 

#### Behavior

Opening Windows Notepad with no currently existing tab(s) will create an empty "Untitled" tab and an associated _No File Tab_ bin file.

Creating new tab(s) will create associated _No File Tab_ bin file(s).

Opening file(s) from disk will create associated _File Tab_ bin file(s).

Closing tab(s) will delete the associated bin file(s). 

TODO: Reasons State Files are created/deleted.

The existence of no bin file(s) indicates:
- Windows Notepad has never been opened
- All tabs have been manually closed
- Manual deletion

If you drag/drop multiple files into Windows Notepad, the internal content of the bin file(s) will not load until the tab becomes active. (To be expanded upon)

#### File Format

##### File Tab
|Name|Type|Notes|
|---|---|---|
|Signature / Magic Bytes|2 bytes|[0x4E, 0x50] "NP"|
|Sequence Number|uLEB128|Always 0|
|TypeFlag|uLEB128|Equal to 1|
|FilePathLength|uLEB128|Length of the FilePath in bytes|
|FilePath|UTF-16LE (Variable Length)|FilePath string with length determined from FilePathLength|
|SavedFileContentLength|uLEB128|Size in bytes of the text file saved on disk|
|EncodingType|1 byte|1 = ANSI / 2 = UTF16LE / 3 = UTF16BE / 4 = UTF8BOM / 5 = UTF8|
|CarriageReturnType|1 byte|1 = Windows CRLF / 2 = Macintosh CR / 3 = Unix LF|
|Timestamp|uLEB128|18-digit Win32 FILETIME [https://learn.microsoft.com/en-us/windows/win32/api/minwinbase/ns-minwinbase-filetime](https://learn.microsoft.com/en-us/windows/win32/api/minwinbase/ns-minwinbase-filetime) / [https://www.epochconverter.com/ldap](https://www.epochconverter.com/ldap)|
|FileHash|32 bytes|SHA256 Hash of the text file saved on disk|
|:question:Unknown|2 bytes|[0x00, 0x01]|
|SelectionStartIndex|uLEB128|Start position of text selection|
|SelectionEndIndex|uLEB128|End position of text selection|
|[Configuration Block](#configuration-block)|||
|ContentLength|uLEB128|Length of the Content in bytes|
|Content|UTF-16LE (Variable Length)|Text Content with length determined from ContentLength. Will be blank if the file has only been opened and has no changes.|
|Unsaved|1 byte|Unsaved flag|
|CRC32|4 bytes|CRC32 Check|
|[Unsaved Buffer Chunks](#unsaved-buffer-chunk)||Values may not exist|

##### No File Tab
|Name|Type|Notes|
|---|---|---|
|Signature / Magic Bytes|2 bytes|[0x4E, 0x50] "NP"|
|Sequence Number|uLEB128|Always 0|
|TypeFlag|uLEB128|Equal to 0|
|:question:Unknown|1 byte|[0x01]|
|SelectionStartIndex|uLEB128|Start position of text selection|
|SelectionEndIndex|uLEB128|End position of text selection|
|[Configuration Block](#configuration-block)|||
|ContentLength|uLEB128|Length of the Content in bytes|
|Content|UTF-16LE (Variable Length)|Text Content with length determined from ContentLength|
|Unsaved|1 byte|Unsaved flag|
|CRC32|4 bytes|CRC32 Check|
|[Unsaved Buffer Chunks](#unsaved-buffer-chunk)||Values may not exist|

##### State File
|Name|Type|Notes|
|---|---|---|
|Signature / Magic Bytes|2 bytes|[0x4E, 0x50] "NP"|
|Sequence Number|uLEB128|Increments and highest number signifies the active state file|
|TypeFlag|uLEB128|Greater than 1|
|:question:Unknown|1 byte|[0x00]|
|BinSize|uLEB128|Size in bytes of the associated *.bin file| 
|SelectionStartIndex|uLEB128|Start position of text selection|
|SelectionEndIndex|uLEB128|End position of text selection|
|[Configuration Block](#configuration-block)|||
|CRC32|4 bytes|CRC32 Check|

###### Configuration Block
|Name|Type|Notes|
|---|---|---|
|WordWrap|1 byte|WordWrap flag|
|RightToLeft|1 byte|RightToLeft flag|
|ShowUnicode|1 byte|ShowUnicode flag|
|Version/MoreOptions|uLEB128|Number of More Options in bytes that follow|
|[More Options Block](#more-options-block)|||

###### More Options Block
|Name|Type|Notes|
|---|---|---|
|:question:Unknown| 1 byte|Spellcheck/Autocorrect? Do not seem to be flags. These were added to the file format when Spellcheck/Autocorrect feature was added|
|:question:Unknown| 1 byte|Spellcheck/Autocorrect? Do not seem to be flags. These were added to the file format when Spellcheck/Autocorrect feature was added|

###### Unsaved Buffer Chunk

|Name|Type|Notes|
|---|---|---|
|Cursor Position|uLEB128|Cursor Position of where Deletion/Addition/Insertion begins. Insertion is signified by both a Deletion Action and Addition Action|
|Deletion Action|uLEB128|Number of Characters deleted|
|Addition Action|uLEB128|Number of Characters added|
|Added Characters|UTF-16LE (Variable Length)|Characters added with length determined from Addition Action|
|CRC32|4 bytes|CRC32 Check of Unsaved Buffer Chunk|

### Windowstate 

> [!NOTE]
> Location of Files
> `%localappdata%\Packages\Microsoft.WindowsNotepad_8wekyb3d8bbwe\LocalState\WindowState`
>
> Relevant Files
> `*.0.bin` `*.1.bin`

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
|Name|Type|Notes|
|---|---|---|
|Signature / Magic Bytes|2 bytes|[0x4E, 0x50] "NP"|
|Sequence Number|uLEB128||
|BytesToCRC|uLEB128|Number of bytes to the CRC Check|
|:question:Unknown|1 byte|[0x00]|
|NumberTabs|uLEB128|Number of Tabs in Notepad|
|GUID Chunks|16 bytes (Variable Number of Chunks)|GUID for each Tab in view order that refer to the filename of the matching [Tabstate](#tabstate) file|
|ActiveTab|uLEB128|Number of Active Tab in Notepad. 0 based index.|
|TopLeftCoords_X|uINT32||
|TopLeftCoords_Y|uINT32||
|BottomRightCoords_X|uINT32||
|BottomRightCoords_Y|uINT32|| 
|WindowSize_Width|uINT32||
|WindowSize_Height|uINT32||
|:question:Unknown|1 byte|[0x00]|
|CRC32|4 bytes|CRC32 Check|
|[Slack Space](#slack-space)|Variable||

#### Slack Space
It appears that the windowstate files will never reduce in size. More testing is required to validate this or to discover what actions will cause them to be deleted or cleared out.

There is a potential to recover complete or partial GUIDs from the slack space that can be tied back to past tabstate files. These deleted tabstate files could possibly be recovered and examined.  

##### Approaches

> [!WARNING]  
> The approaches make heavy assumptions. As Tabs are opened and closed, the slack space will get more and more convoluted and disarrayed. Manual parsing is suggested and there is no guarantee of being able to recover anything of use. 

WIP

### Settings
> [!NOTE]
> Location of Files
> `%localappdata%\Packages\Microsoft.WindowsNotepad_8wekyb3d8bbwe\Settings`
>
> Relevant Files
> `settings.dat`

The settings files store application wide settings and defaults. The `settings.dat` file is an application hive which can be opened with RegEdit and other tools which can handle registry files. There is a Binary Template file for 010 Editor that I've updated. 

#### Useful Links / Information

[Application Hives](https://learn.microsoft.com/en-us/windows-hardware/drivers/kernel/filtering-registry-operations-on-application-hives)

[Windows Store App Settings](https://lunarfrog.com/blog/inspect-app-settings)

[Manipulating Windows Store App Settings](https://www.damirscorner.com/blog/posts/20150117-ManipulatingSettingsDatFileWithSettingsFromWindowsStoreApps.html)

[UWP App Data Storage](https://helgeklein.com/blog/uwp-universal-windows-app-data-storage-admins/)

[REGF Format](https://github.com/libyal/libregf/blob/main/documentation/Windows%20NT%20Registry%20File%20(REGF)%20format.asciidoc)

[Registry Format](https://github.com/msuhanov/regf/blob/master/Windows%20registry%20file%20format%20specification.md)

#### Behavior

If a key doesn't exist that option hasn't been changed from the default or set. 

#### File Format

Last 8 bytes of each key are the FileTime. This appears in the value of the key.

| Type | Hex | Description |
|---|---|---|
|0x5f5e104|`04 E1 F5 05` | uINT32
|0x5f5e105|`05 E1 F5 05` | uINT32
|0x5f5e10b|`0B E1 F5 05` | byte (bool)
|0x5f5e10c|`0C E1 F5 05` | string (NULL Terminated)


SCREENSHOT HERE


| KeyName | Type | Notes |
|---|---|---|
|AutoCorrect|0x5f5e10b| `00` Off / `01` On
|FontFamily|0x5f5e10c| String
|FontStyle|0x5f5e10c| String
|GhostFile|0x5f5e10b| `00` Open in a new window / `01` Open content from a previous session
|LocalizedFontFamily|0x5f5e10c| String
|LocalizedFontStyle|0x5f5e10c| String
|OpenFile|0x5f5e104| `00` New Tab / `01` New Window
|SpellCheckState|0x5f5e10c| JSON: `{"Enabled":false,"FileExtensionsOverrides":[[".md",true],[".ass",true],[".lic",true],[".srt",true],[".lrc",true],[".txt",true]]}`
|StatusBarShown|0x5f5e10b| `00` Off / `01` On
|TeachingTipCheckCount|0x5f5e105| Unknown
|TeachingTipExplicitClose|0x5f5e10b| Unknown
|TeachingTipVersion|0x5f5e105| Unknown
|Theme|0x5f5e104| `00` System / `01` Light / `02` Dark
|WindowPositionBottom|0x5f5e104|
|WindowPositionHeight|0x5f5e104|
|WindowPositionLeft|0x5f5e104|
|WindowPositionRight|0x5f5e104|
|WindowPositionTop|0x5f5e104|
|WindowPositionWidth|0x5f5e104|
|WordWrap|0x5f5e10b| `00` Off / `01` On

## Acknowledgements

In random order:

[jlogsdon](https://github.com/jlogsdon)   
[NordGaren](https://github.com/Nordgaren)  
[JohnHammond](https://github.com/JohnHammond)   
[JustArion](https://github.com/JustArion)  
[joost-j](https://github.com/joost-j)   
[daddycocoaman](https://github.com/daddycocoaman)