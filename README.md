# Notepad State Library

Microsoft Windows 11's version of [Windows Notepad](https://apps.microsoft.com/detail/9msmlrh6lzf3) supports multiple tabs and many other features. This repository serves to record and document my research and efforts in reverse engineering the format of the tabstate and windowstate files and understand their behavior. The result of which is a C# library and toolset that provides the ability to parse and manipulate the tabstate, windowstate, and settings files. 

The following are planned:
- [ ] Tabstate Manipulator
- [ ] Windowstate Manipulator
- [ ] Ways to detect manipulation
- [X] POC Malware - [https://github.com/ogmini/Notepad-State-Library/releases/tag/GaslitPad](https://github.com/ogmini/Notepad-State-Library/releases/tag/GaslitPad)

This library and its tools could be useful in forensic investigations or even in the toolbox for a red/purple team.

> [!NOTE]
> This repository grew out of two previous repositories of research and code that have been kept for posterity. 
>  
> [https://github.com/ogmini/Notepad-Tabstate-Buffer](https://github.com/ogmini/Notepad-Tabstate-Buffer)  
> [https://github.com/ogmini/Notepad-Windowstate-Buffer](https://github.com/ogmini/Notepad-Windowstate-Buffer)

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

```
 -t, --tabstatelocation       Tab State Folder Location. Default value is the system location.

 -w, --windowstatelocation    Window State Folder Location. Default value is the system location.

 -o, --outputlocation         Output Folder Location for CSV files. Default location is same folder as program.

 --help                       Display this help screen.

 --version                    Display version information.
 ```

 Examples

The following arguments would look at D:\tabstatefolder for tab state files and D:\windowstatefolder for window state fles. The csv files will be output to D:\results.   
 `WindowsNotepadParser.exe -t D:\tabstatefolder -w D:\windowstatefolder -o D:\results`

The following arguments will parse the default system location for the current user and output the csv files to D:\results.  
 `WindowsNotepadParser.exe -o D:\results`

## Information

The information below has been tested/validated on the following configurations:

| Windows Build | Windows Notepad Version 
|---|---|
| Windows 11 23H2 OS Build 22635.3566 (Beta Release Branch) | 11.2402.22.0
| Windows 11 23H2 OS Build 22631.3527 (Stable Release Branch) | 11.2402.22.0
| Windows 11 23H2 OS Build 22631.3737 (Stable Release Branch) | 11.2404.10.0
| Windows 11 23H2 OS Build 22631.4317 (Stable Release Branch) | 11.2407.9.0
| Windows 11 23H2 OS Build 22631.4317 (Stable Release Branch) | 11.2408.12.0
| Windows 11 23H2 OS Build 22631.4460 (Stable Release Branch) | 11.2409.9.0


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
		- Unsaved condition is visually denoted by a dot to the right of the Tab name.   
		![Dot](/Images/Unsaved%20Dot.png)
	- They have a TypeFlag of 1. 
- _No File Tab_
	- These tabs have not been saved to disk and have not been opened from a file on disk. They only exist in the *.bin files. 
	- These tabs can be in a New or Reopened condition.
		- Reopened condition is visually denoted by a dot to the right of the Tab name.   
		![Dot](/Images/Unsaved%20Dot.png)
	- They have a TypeFlag of 0.
- _State File_
	- These are the *.0.bin and *.1.bin files and store extra information about the related matching GUID *.bin. 
	- These files do not always exist and this behavior will be expanded upon in the [Behavior](#behavior) section. 
	- They have a TypeFlag of 10 or 11.

Integrity of the file is validated with CRC32 calculated and stored for the preceding bytes. 

I've created a [Notepad-Tabstate.bt](https://www.sweetscape.com/010editor/repository/templates/file_info.php?file=Notepad-TabState.bt&type=0&sort=) for 010 Editor and a [Notepad-Tabstate.hexpat](/PatternFiles/Tabstate/Notepad-TabState.hexpat) for ImHex to assist in examining these files.

#### File Format

##### File Tab
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

##### No File Tab
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

##### State File
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

The windowstate files store information about opened windows of Windows Notepad and files are created for each opened window. Information is stored about:

- Number of tabs
- Order of tabs
- Active tab
- Window size
- Window position

Integrity of the file is validated with CRC32 calculated and stored for the preceding bytes.

I've created a [Notepad-WindowState.bt](https://www.sweetscape.com/010editor/repository/templates/file_info.php?file=Notepad-WindowState.bt&type=0&sort=) for 010 Editor and a [Notepad-WindowState.hexpat](/PatternFiles/Windowstate/Notepad-WindowState.hexpat) for ImHex to assist in examining these files.

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

#### Slack Space
It appears that the windowstate files will never reduce in size. More testing is required to validate this or to discover what actions will cause them to be deleted or cleared out.

There is a potential to recover complete or partial GUIDs from the slack space that can be tied back to past [Tabstate](#tabstate) files. These deleted files could possibly be recovered and examined. As Tabs are opened and closed, the slack space will get more and more convoluted and disarrayed as records are overwritten as the GUID Chunks section changes in size. Manual parsing is suggested and there is no guarantee of being able to recover anything of use.   

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

The settings files store application wide settings and defaults. The `settings.dat` file is an application hive which can be opened with Registry Editor and other tools which can handle registry files. I've also updated the [RegistryHive.bt](https://www.sweetscape.com/010editor/repository/templates/file_info.php?file=RegistryHive.bt&type=0&sort=) for 010 Editor. If a key doesn't exist that option hasn't been changed from the default or set. Research has already been published on this file format and a list of links can be found [here](#useful-links--information)

#### Useful Links / Information

[Application Hives](https://learn.microsoft.com/en-us/windows-hardware/drivers/kernel/filtering-registry-operations-on-application-hives)

[Windows Store App Settings](https://lunarfrog.com/blog/inspect-app-settings)

[Manipulating Windows Store App Settings](https://www.damirscorner.com/blog/posts/20150117-ManipulatingSettingsDatFileWithSettingsFromWindowsStoreApps.html)

[UWP App Data Storage](https://helgeklein.com/blog/uwp-universal-windows-app-data-storage-admins/)

[REGF Format](https://github.com/libyal/libregf/blob/main/documentation/Windows%20NT%20Registry%20File%20(REGF)%20format.asciidoc)

[Registry Format](https://github.com/msuhanov/regf/blob/master/Windows%20registry%20file%20format%20specification.md)

#### File Format
| Type | Hex | Description |
|---|---|---|
|0x5f5e104|`04 E1 F5 05` | uINT32
|0x5f5e105|`05 E1 F5 05` | uINT32
|0x5f5e10b|`0B E1 F5 05` | byte (bool)
|0x5f5e10c|`0C E1 F5 05` | string (NULL Terminated)

Last 8 bytes of the value for each key is the 18-digit Win32 FILETIME Timestamp for the setting change.

| KeyName | Type | Notes |
|---|---|---|
|AutoCorrect|0x5f5e10b| 0 = Off / 1 = On
|FindMatchCase | 0x5f5e10b | 0 = Off / 1 = On. Default is 0. |
|FindString | 0x5f5e10c | Stores the last string searched by find. |
|FindWrapAround | 0x5f5e10b | 0 = Off / 1 = On. Default is 1. |
|FontFamily|0x5f5e10c| String
|FontStyle|0x5f5e10c| String
|GhostFile|0x5f5e10b| 0 = Open in a new window / 1 = Open content from a previous session
|LocalizedFontFamily|0x5f5e10c| String
|LocalizedFontStyle|0x5f5e10c| String
|OpenFile|0x5f5e104| 0 = New Tab / 1 = New Window
|RecentFiles | 0x5f5e10c | CSV array. List is in descending order with the most recently closed file at the top. |
|RecentFilesFirstLoad | 0x5f5e10b | 0 = Off / 1 = On |
|ReplaceString | 0x5f5e10c | Stores the last string that was the replacement. |
|RewriteEnabled|0x5f5e10b| 0 = Off / 1 = On
|RewriteTeachingtip|0x5f5e10b| 0 = Off / 1 = On
|SpellCheckState|0x5f5e10c| JSON: `{"Enabled":false,"FileExtensionsOverrides":[[".md",true],[".ass",true],[".lic",true],[".srt",true],[".lrc",true],[".txt",true]]}`
|StatusBarShown|0x5f5e10b| 0 = Off / 1  = On
|TeachingTipCheckCount|0x5f5e105| Unknown
|TeachingTipExplicitClose|0x5f5e10b| Unknown
|TeachingTipVersion|0x5f5e105| Unknown
|Theme|0x5f5e104| 0 = System / 1 = Light / 2 = Dark
|WebAccountId|0x5f5e10c| Unknown
|WindowPositionBottom|0x5f5e104|
|WindowPositionHeight|0x5f5e104|
|WindowPositionLeft|0x5f5e104|
|WindowPositionRight|0x5f5e104|
|WindowPositionTop|0x5f5e104|
|WindowPositionWidth|0x5f5e104|
|WordWrap|0x5f5e10b| 0 = Off / 1 = On|

### Behavior

Timestamp for the _File Tab_ for [Tabstate](#tabstate) files will be set to 0 when the file is saved to disk. It will have a valid value when the first change is made to the contents of the file. Any successive changes before saving the file will not result in the Timestamp being updated. This behavior persists over opening/closing Windows Notepad. In short, the Timestamp for the _File Tab_ indicates when changes were started to be made on the file and that they have not yet been saved to disk.

The timestamps associated with each key in the application hive show us when those [Settings](#settings) were last changed. 

The sequence number is used to tell which *.0.bin or *.1.bin is active as updates alternate between the two. The file with the highest sequence number is the active one and are relevant to both _State Files_ and [Windowstate](#windowstate) files. 

The presence of _State Files_ can tell us a bit about the usage pattern of Windows Notepad. For a _File Tab_ with no changes, the _State Files_ are only created when Windows Notepad is closed. They are subsequently deleted when the _File Tab_ is made active. The sequence number for the _State Files_ will never increment and the *.1.bin file will be empty.

For a _File Tab_ or _No File Tab_ with unsaved changes, the _State Files_ are only created when Windows Notepad is closed and no [Unsaved Buffer Chunks](#unsaved-buffer-chunk) were flushed. They are subsequently deleted when new changes are made or the file has been saved. The sequence number for the _State Files_ will increment everytime Windows Notepad is closed and is indicative of many cycles of opening and closing Windows Notepad while in the unsaved and flushed state. 

While Windows Notepad is open the _File Tab_ and _No File Tab_ can have [Unsaved Buffer Chunks](#unsaved-buffer-chunk) of changes that haven't been flushed. The [Unsaved Buffer Chunks](#unsaved-buffer-chunk) can be used to playback the changes to the text similar to a keylogger. Once Windows Notepad is closed or the file is saved, the [Unsaved Buffer Chunks](#unsaved-buffer-chunk) are flushed into the content.

Opening a Tab adds another Tab GUID Chunk to the collection of Chunks and updates the number of bytes to the CRC32 in the [Windowstate](#windowstate) file. Any existing slack space in the file will get overwritten up to the end of the new CRC32. 

Closing a tab deletes the relevant Tab GUID Chunk from the collection of Chunks and updates the number of bytes to the CRC32. Slack space after the CRC32 may result from closing tabs. The files appear to never get smaller.

The following actions will cause an update of the sequence number in the [Windowstate](#windowstate) files:
- Resizing window
- Moving window
- Reordering/moving tabs
- Closing tab(s)
	- Closing multiple tabs at once results in one action
- Opening tab(s)

Creating a new Windows Notepad window by dragging a tab outside of the original window will spawn new [Windowstate](#windowstate) files. As you close each extra window, it will prompt you to save any files in that window and the corresponding [Windowstate](#windowstate) files will be deleted. When the last window of Windows Notepad is closed, the final [Windowstate](#windowstate) files will not be deleted. Only the [Windowstate](#windowstate) files for the last closed Windows Notepad is kept.

## Acknowledgements

In random order:

[jlogsdon](https://github.com/jlogsdon)   
[NordGaren](https://github.com/Nordgaren)  
[JohnHammond](https://github.com/JohnHammond)   
[JustArion](https://github.com/JustArion)  
[joost-j](https://github.com/joost-j)   
[daddycocoaman](https://github.com/daddycocoaman)
