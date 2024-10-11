# Is that Windows Notepad window really empty?

## Synposis

### Forensics Question:  
What artifacts can be recovered from Windows Notepad now that it supports multiple tabs, saving session state, and multi-level undo?   
What evidence can be lost if you close Windows Notepad?   
What evidence can be lost if you open Windows Notepad?

### OS Version:  
- Microsoft Windows 11 23H2 Build 22631 (Original Tests)
- Microsoft Windows 11 23H2 Build 22635 (Original Tests)
### Tools:
- ImHex
- 010 Editor
- RegEdit

The full research and tools to assist in artifact recovery can be found at [https://github.com/ogmini/Notepad-State-Library](https://github.com/ogmini/Notepad-State-Library)

## Background
Windows Notepad is the default text editor included with standard installations of Windows 11, with updates available through the Windows App Store. It is commonly used for quickly editing and reading text files, as well as for taking notes. Microsoft has begun enhancing its features, adding support for multiple tabs, saving session states, and multi-level undo.

To accommodate these new functionalities, Windows Notepad must store this information. This paper will explore the artifacts that can be recovered from the local filesystem, identify their locations, and explain how to read and understand them. Additionally, it will discuss preservation methods and the relevance of these artifacts to digital forensics.

![Screenshot of Notepad](/Images/Notepad.png)

https://blogs.windows.com/windows-insider/2023/08/31/new-updates-for-snipping-tool-and-notepad-for-windows-insiders/  
https://blogs.windows.com/windows-insider/2023/01/19/tabs-in-notepad-begins-rolling-out-to-windows-insiders/  
https://blogs.windows.com/windows-insider/2021/12/07/redesigned-notepad-for-windows-11-begins-rolling-out-to-windows-insiders/

## Artifacts

There are three types of artifacts to be aware of when examining Windows Notepad that can found in different locations. 

> - [Tabstate](#tabstate) - Stores information specific open tabs in Windows Notepad
> - [Windowstate](#windowstate) - Stores information about the Windows Notepad window 
> - [Settings](#settings) - Stores application wide settings  

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
    - These tabs can be in a Saved or Unsaved condition.
    - They have a TypeFlag of 1. 
- _No File Tab_
    - These tabs have not been saved to disk and have not been opened from a file on disk. They only exist in the *.bin files. 
    - They have a TypeFlag of 0.
- _State File_
    - These are the *.0.bin and *.1.bin files and store extra information about the related matching GUID *.bin. 
    - They have a TypeFlag greater than 1.

Both the _File Tab_ and _No File Tab_ can have related _State Files_. EXPAND/CHECK UPON THIS

Notepad is open with SavedFileNoChanges and SavedFileChanges. No state files. Closing Notepad while active tab = SavedFileNoChanges creates state files for the SavedFileNoChanges. Reopening Windows Notepad deletes the state files. Cycle Repeats.

Closing Notepad while active tab = SavedFileChanges results in creating both state files. Reopening makes no changes. Closing makes no changes. 

Changing the active tab to the SavedFileNoChanges deletes its state files

Changing the active tab to the SavedFileChanges makes no changes

While Windows Notepad is open the _File Tab_ and _No File Tab_ can have [Unsaved Buffer Chunks](#unsaved-buffer-chunk) of changes that haven't been flushed. The [Unsaved Buffer Chunks](#unsaved-buffer-chunk) can be used to playback the changes to the text similar to a keylogger. Once Windows Notepad is closed or the file is saved, the [Unsaved Buffer Chunks](#unsaved-buffer-chunk) are flushed into the content. VERIFY THIS (It appears that unsaved buffer chunks for a File Tab that has unsaved changes will persist)

Integrity of the file is validated with CRC32. 

#### File Tab Format
|Name|Type|Notes|Saved Condition|
|---|---|---|---|
|Signature / Magic Bytes|2 bytes|[0x4E, 0x50] "NP"|
|Sequence Number|uLEB128|Always 0|
|TypeFlag|uLEB128|Equal to 1|
|FilePathLength|uLEB128|Length of the FilePath in bytes|
|FilePath|UTF-16LE (Variable Length)|FilePath string with length determined from FilePathLength|
|SavedFileContentLength|uLEB128|Size in bytes of the text file saved on disk|Will be 0 if all changes have been saved to the file|
|EncodingType|1 byte|1 = ANSI / 2 = UTF16LE / 3 = UTF16BE / 4 = UTF8BOM / 5 = UTF8|
|CarriageReturnType|1 byte|1 = Windows CRLF / 2 = Macintosh CR / 3 = Unix LF|
|Timestamp|uLEB128|18-digit Win32 FILETIME|Will be 0 if all changes have been saved to the file|
|FileHash|32 bytes|SHA256 Hash of the text file saved on disk|Will be 0 if all changes have been saved to the file|
|:question:Unknown|2 bytes|[0x00, 0x01]|
|SelectionStartIndex|uLEB128|Start position of text selection|
|SelectionEndIndex|uLEB128|End position of text selection|
|[Configuration Block](#configuration-block)|||
|ContentLength|uLEB128|Length of the Content in bytes|Will be 0 if all changes have been saved to the file|
|Content|UTF-16LE (Variable Length)|Text Content with length determined from ContentLength|Will not exist if all changes have been saved to the file|
|Unsaved|1 byte|Unsaved flag|Will be 0 since the file has been saved|
|CRC32|4 bytes|CRC32 Check|
|[Unsaved Buffer Chunks](#unsaved-buffer-chunk)||Will exist if any changes to the file are unsaved|Will not exist if all changes been saved to the file|

![010 Editor view of *.bin for opened file with no changes](/Images/Saved%20File%20-%20Read%20Only.png)  
![010 Editor view of *.bin for opened file with unsaved changed](/Images/Saved%20File%20-%20Changes.png)


#### No File Tab Format
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

#### State File Format
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

#### Configuration Block Format
|Name|Type|Notes|
|---|---|---|
|WordWrap|1 byte|WordWrap flag|
|RightToLeft|1 byte|RightToLeft flag|
|ShowUnicode|1 byte|ShowUnicode flag|
|Version/MoreOptions|uLEB128|Number of More Options in bytes that follow|
|[More Options Block](#more-options-block)|||

#### More Options Block Format
|Name|Type|Notes|
|---|---|---|
|:question:Unknown| 1 byte|Spellcheck/Autocorrect? Do not seem to be flags. These were added to the file format when Spellcheck/Autocorrect feature was added|
|:question:Unknown| 1 byte|Spellcheck/Autocorrect? Do not seem to be flags. These were added to the file format when Spellcheck/Autocorrect feature was added|

#### Unsaved Buffer Chunk Format

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

## Reading and Understanding Artifacts

## Preservation

## Relevance

## Conclusion
