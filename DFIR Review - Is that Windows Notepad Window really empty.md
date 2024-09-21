# Is that Windows Notepad window really empty?

## SYNOPSIS

### Forensics Question:  
What artifacts can we recover from Windows Notepad now that it supports multiple tabs, saving session state, and multi-level undo?   
What evidence can be lost if you close Windows Notepad?   
What evidence can be lost if you open Windows Notepad?

### OS Version:  
- Microsoft Windows 11 23H2 Build 22631 (Original Tests)
- Microsoft Windows 11 23H2 Build 22635 (Original Tests)
### Tools:
- ImHex
- 010 Editor

The full research and code can be found at [https://github.com/ogmini/Notepad-State-Library](https://github.com/ogmini/Notepad-State-Library)

## Background
Windows Notepad is the default text editor included with standard installations of Windows 11 with updates being pushed through the Windows App Store. As a result, it is often used for quickly editing text files, reading text files, and taking notes. Microsoft has started to extend the features by supporting multiple tabs, saving session state, and multi-level undo. In order to support these features and complexity, Windows Notepad must be storing information and this paper will show what artifacts can be recovered from the local filesystem.

https://blogs.windows.com/windows-insider/2023/08/31/new-updates-for-snipping-tool-and-notepad-for-windows-insiders/
https://blogs.windows.com/windows-insider/2023/01/19/tabs-in-notepad-begins-rolling-out-to-windows-insiders/
https://blogs.windows.com/windows-insider/2021/12/07/redesigned-notepad-for-windows-11-begins-rolling-out-to-windows-insiders/

## Research

## Findings

## Tool
