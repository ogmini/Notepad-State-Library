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
- Registry Editor

The full research and tools to assist in artifact recovery can be found at [https://github.com/ogmini/Notepad-State-Library](https://github.com/ogmini/Notepad-State-Library)

## Background
Windows Notepad is the default text editor included with standard installations of Windows 11, with updates available through the Windows App Store. It is commonly used for quickly editing and reading text files, as well as for taking notes. Microsoft has begun enhancing its features, adding support for multiple tabs, saving session states, and multi-level undo.

To accommodate these new functionalities, Windows Notepad must store this information. We will explore the artifacts that can be recovered from the local filesystem, identify their locations, and explain how to read and understand them. Additionally, we will discuss the behavior and relevance of these artifacts to digital forensics.

![Screenshot of Notepad](/Images/Notepad.png)   
Seemingly empty Windows Notepad...  

![Recovered Text](/Images/SmokingGun.gif)   
Analysis results in the above recovered text (I have found the SMOKING GUN) as typed 

https://blogs.windows.com/windows-insider/2023/08/31/new-updates-for-snipping-tool-and-notepad-for-windows-insiders/  
https://blogs.windows.com/windows-insider/2023/01/19/tabs-in-notepad-begins-rolling-out-to-windows-insiders/  
https://blogs.windows.com/windows-insider/2021/12/07/redesigned-notepad-for-windows-11-begins-rolling-out-to-windows-insiders/

## Artifacts

There are three types of artifacts to be aware of when examining Windows Notepad that can found in different locations. 

- [Tabstate](#tabstate) - Stores information specific open tabs in Windows Notepad
- [Windowstate](#windowstate) - Stores information about the Windows Notepad window 
- [Settings](#settings) - Stores application wide settings  

The information below is current as of Windows Notepad Version 11.2408.12.0

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
    - These tabs can be in a New or Reopened condition.
    - They have a TypeFlag of 0.
- _State File_
    - These are the *.0.bin and *.1.bin files and store extra information about the related matching GUID *.bin. 
    - These files do not always exist and this behavior will be expanded upon in the [Behavior](#behavior) section. 
    - They have a TypeFlag of 10 or 11.

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

The image below displays an example of a file with no changes that is in the Saved condition. Take note of the values for SavedFileContentLength, Timestamp, FileHash, ContentLength, and Unsaved. 
![010 Editor view of *.bin for opened file with no changes](/Images/Saved%20File%20-%20Read%20Only.png)  
The image below displays an example of a file with unsaved changes that is in the Unsaved condition. Take note of the values for SavedFileContentLength, Timestamp, FileHash, ContentLength,  Content, Unsaved, and Unsaved Buffer Chunks.  
![010 Editor view of *.bin for opened file with unsaved changed](/Images/Saved%20File%20-%20Changes.png)


#### No File Tab Format
|Name|Type|Notes|New Condition|
|---|---|---|---|
|Signature / Magic Bytes|2 bytes|[0x4E, 0x50] "NP"|
|Sequence Number|uLEB128|Always 0|
|TypeFlag|uLEB128|Equal to 0|
|:question:Unknown|1 byte|[0x01]|
|SelectionStartIndex|uLEB128|Start position of text selection|
|SelectionEndIndex|uLEB128|End position of text selection|
|[Configuration Block](#configuration-block)|||
|ContentLength|uLEB128|Length of the Content in bytes|Will be 0|
|Content|UTF-16LE (Variable Length)|Text Content with length determined from ContentLength|Will not exist|
|Unsaved|1 byte|Unsaved flag|Will be 0
|CRC32|4 bytes|CRC32 Check|
|[Unsaved Buffer Chunks](#unsaved-buffer-chunk)||Values will exist for changes until they are flushed to Content when Windows Notepad is closed||

The image below displays an example of a newly created tab in the New condition with text changes. Take note of the values for ContentLength, Unsaved, and Unsaved Buffer Chunks.  
![010 Editor view of *.bin for new tab](/Images//Unsaved%20File%20-%20New.png)  
The image below displays an example of a tab in the Reopened condition with no text changes. Take note of the values for ContentLength, Content, Unsaved, and Unsaved Buffer Chunks.  
![010 Editor view of *.bin for reopened tab](/Images/Unsaved%20File%20-%20Reopened.png)  
The image below displays an example of a tab in the Reopened condition with text changes. Take note of the values for ContentLength, Content, Unsaved, and Unsaved Buffer Chunks.  
![010 Editor view of *.bin for reopened tab](/Images/Unsaved%20File%20-%20Reopened%20with%20changes.png)


#### State File Format
|Name|Type|Notes|
|---|---|---|
|Signature / Magic Bytes|2 bytes|[0x4E, 0x50] "NP"|
|Sequence Number|uLEB128|Increments and highest number signifies the active state file|
|TypeFlag|uLEB128|10 = No File Tab State / 11 = File Tab State|
|:question:Unknown|1 byte|[0x00]|
|BinSize|uLEB128|Size in bytes of the associated *.bin file| 
|SelectionStartIndex|uLEB128|Start position of text selection|
|SelectionEndIndex|uLEB128|End position of text selection|
|[Configuration Block](#configuration-block)|||
|CRC32|4 bytes|CRC32 Check|

The image below displays an example of the state file for a No File Tab. Take note of the TypeFlag.   
![010 Editor view of the state file for a No File Tab](/Images/State%20-%20No%20File.png)  
The image below displays an example of the state file for a File Tab. Take note of the TypeFlag.   
![010 Editor view of the state file for a File Tab](/Images/State%20-%20File.png)

#### Configuration Block Format
|Name|Type|Notes|
|---|---|---|
|WordWrap|1 byte|WordWrap flag|
|RightToLeft|1 byte|RightToLeft flag|
|ShowUnicode|1 byte|ShowUnicode flag|
|Version/MoreOptions|uLEB128|Number of More Options in bytes that follow|
|[More Options Block](#more-options-block)|||

The image below shows the WordWrap option in Windows Notepad. This setting is specific to the Tab.  
![Windows Notepad Settings View](/Images/Settings.png)  
The image below shows the RightToLeft and ShowUnicode options in the Right-Click menu for Windows Notepad. These settings are specific to the Tab.  
![Windows Notepad Settings View](/Images/RightClickOptions.png)  

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

The image below shows the Unsaved Buffer Chunk for pasting a block of text. This is also similar to typing individual letters.  
![010 Editor view of Unsaved Buffer Chunk for pasting a block of text](/Images/Pasted%20Text%20Block.png)  
The image below shows the Unsaved Buffer Chunk for deleting a block of text. This is also similar to deleting individual letters.  
![010 Editor view of Unsaved Buffer Chunk for deleting a block of text](/Images/Deleted%20Text%20Block.png)  
The image below shows the Unsaved Buffer Chunk for overwriting a block of text. This is also similar to inserting individual letters.  
![010 Editor view of Unsaved Buffer Chunk for overwriting a block of text](/Images/Overwrite%20Text.png)  

### Windowstate 

> [!NOTE]
> Location of Files
> `%localappdata%\Packages\Microsoft.WindowsNotepad_8wekyb3d8bbwe\LocalState\WindowState`
>
> Relevant Files
> `*.0.bin` `*.1.bin`

The windowstate files store information about opened windows of Windows Notepad and files are created for each opened window. Information is stored about:

- Number of tabs
- Order of tabs
- Active tab
- Window size
- Window position

Integrity of the file is validated with CRC32. 

#### File Format
|Name|Type|Notes|
|---|---|---|
|Signature / Magic Bytes|2 bytes|[0x4E, 0x50] "NP"|
|Sequence Number|uLEB128||
|BytesToCRC|uLEB128|Number of bytes to the CRC Check|
|:question:Unknown|1 byte|[0x00]|
|NumberTabs|uLEB128|Number of Tabs in Notepad|
|GUID Chunks|16 bytes (Variable Number of Chunks)|GUID for each tab in view order that refer to the filename of the matching [Tabstate](#tabstate) file|
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

The image below shows the Windowstate for Windows Notepad with three tabs and the middle tab being active.  
![010 Editor view of Windowstate for multiple tabs](/Images/Windowstate.png)  

#### Slack Space
It appears that the windowstate files will never reduce in size. More testing is required to validate this or to discover what actions will cause them to be deleted or cleared out.

There is a potential to recover complete or partial GUIDs from the slack space that can be tied back to past [Tabstate](#tabstate) files. These deleted files could possibly be recovered and examined. As Tabs are opened and closed, the slack space will get more and more convoluted and disarrayed as records are overwritten as the GUID Chunks section changes in size. Manual parsing is suggested and there is no guarantee of being able to recover anything of use.   

### Settings
> [!NOTE]
> Location of Files
> `%localappdata%\Packages\Microsoft.WindowsNotepad_8wekyb3d8bbwe\Settings`
>
> Relevant Files
> `settings.dat`

The settings files store application wide settings and defaults. The `settings.dat` file is an application hive which can be opened with Registry Editor and other tools which can handle registry files. I've also updated the [RegistryHive.bt](https://www.sweetscape.com/010editor/repository/templates/file_info.php?file=RegistryHive.bt&type=0&sort=) for 010 Editor. If a key doesn't exist that option hasn't been changed from the default or set. 

#### File Format

| Type | Hex | Description |
|---|---|---|
|0x5f5e104|`04 E1 F5 05` | uINT32
|0x5f5e105|`05 E1 F5 05` | uINT32
|0x5f5e10b|`0B E1 F5 05` | byte (bool)
|0x5f5e10c|`0C E1 F5 05` | string (NULL Terminated)

Last 8 bytes of the value for each key is the Timestamp for the setting change.

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

The image below shows the OpenFile Key which is an example of the 0x5f5e104 Type.   
![010 Editor view of OpenFile Key](/Images/OpenFile.png)  
The image below shows the TeachingTipVersion Key which is an example of the 0x5f5e105 Type.     
![010 Editor view of TeachingTipVersion Key](/Images/TeachingTipVersion.png)  
The image below shows the AutoCorrect Key which is an example of the 0x5f5e10b Type.   
![010 Editor view of AutoCorrect Key](/Images/AutoCorrect.png)  
The image below shows the FontFamil Key which is an example of the 0x5f5e10c Type.   
![010 Editor view of FontFamily Key](/Images/FontFamily.png)  
The image below shows application hive viewed using Registry Viewer. Take note of the Data column as the last 8 bytes are the Timestamp for that key.  
![Registry Viewer of the application hive](/Images/RegistryViewer.png)  

#### Useful Links / Information

[Application Hives](https://learn.microsoft.com/en-us/windows-hardware/drivers/kernel/filtering-registry-operations-on-application-hives)

[Windows Store App Settings](https://lunarfrog.com/blog/inspect-app-settings)

[Manipulating Windows Store App Settings](https://www.damirscorner.com/blog/posts/20150117-ManipulatingSettingsDatFileWithSettingsFromWindowsStoreApps.html)

[UWP App Data Storage](https://helgeklein.com/blog/uwp-universal-windows-app-data-storage-admins/)

[REGF Format](https://github.com/libyal/libregf/blob/main/documentation/Windows%20NT%20Registry%20File%20(REGF)%20format.asciidoc)

[Registry Format](https://github.com/msuhanov/regf/blob/master/Windows%20registry%20file%20format%20specification.md)

## Behavior

The presence of state files can tell us a bit about the usage pattern of Windows Notepad. For a _File Tab_ with no changes, the state files are only created when Windows Notepad is closed. They are subsequently deleted when the _File Tab_ is made active. The Sequence Number for the state files will never increment and the *.1.bin file will be empty.

For a _File Tab_ or _No File Tab_ with unsaved changes, the state files are only created when Windows Notepad is closed and no [Unsaved Buffer Chunks](#unsaved-buffer-chunk) were flushed. They are subsequently deleted when new changes are made or the file has been saved. The sequence number for the state files will increment everytime Windows Notepad is closed and is indicative of many cycles of opening and closing Windows Notepad while in the unsaved and flushed state. 

While Windows Notepad is open the _File Tab_ and _No File Tab_ can have [Unsaved Buffer Chunks](#unsaved-buffer-chunk) of changes that haven't been flushed. The [Unsaved Buffer Chunks](#unsaved-buffer-chunk) can be used to playback the changes to the text similar to a keylogger. Once Windows Notepad is closed or the file is saved, the [Unsaved Buffer Chunks](#unsaved-buffer-chunk) are flushed into the content.

Opening a Tab adds another Tab GUID Chunk to the collection of Chunks and updates the number of bytes to the CRC32 in the [Windowstate](#windowstate) file. Any existing slack space in the file will get overwritten up to the end of the new CRC32. 

Closing a tab deletes the relevant Tab GUID Chunk from the collection of Chunks and updates the number of bytes to the CRC32. Slack space after the CRC32 may result from closing tabs. The files appear to never get smaller.

The sequence number is used to tell which *.0.bin or *.1.bin is active as updates alternate between the two. The file with the highest sequence number is the active one and are relevant to both State Files and [Windowstate](#windowstate) files. 

The following actions will cause an update of the sequence number in the [Windowstate](#windowstate) files:
- Resizing window
- Moving window
- Reordering/moving tabs
- Closing tab(s)
    - Closing multiple tabs at once results in one action
- Opening tab(s)

Creating a new Windows Notepad window by dragging a tab outside of the original window will spawn new [Windowstate](#windowstate) files. As you close each extra window, it will prompt you to save any files in that window and the corresponding [Windowstate](#windowstate) files will be deleted. When the last window of Windows Notepad is closed, the final [Windowstate](#windowstate) files will not be deleted. Only the [Windowstate](#windowstate) files for the last closed Windows Notepad is kept.

## Conclusion

As we have seen, properly preserving artifacts should continue to be a prime concern. Just the act of opening, closing, or interacting Windows Notepad will cause changes in the [Tabstate](#tabstate) and [Windowstate](#windowstate) files. Closing Windows Notepad will cause the loss of any [Unsaved Buffer Chunks](#unsaved-buffer-chunk) which could have contained information such as a password typed by a user and later deleted. Opening Windows Notepad can result in the deletion of [Tabstate](#tabstate) and [Windowstate](#windowstate) files related to files that no longer exist or are inaccessible. A user could have had a text file opened from a USB stick that has gone missing. Changing the active tab or moving the cursor will also result in changes.  

Windows Notepad should not be overlooked as a potential source of evidence due to its ubiquity on Windows 11. It is an easy way for users to quickly save and type notes or have a scratch space that saves itself automatically. A potential criminal could be keeping a running list of victim names and locations in an unsaved tab in Windows Notepad. With the knowledge above, an investigator would know how to preserve and extract relevant artifacts from the file system. Possibly even being able to "replay" the changes and edits to the list.

This research and corresponding tools will continued to be updated as changes are made that impact the artifacts. The latest version can be found at [https://github.com/ogmini/Notepad-State-Library](https://github.com/ogmini/Notepad-State-Library)

