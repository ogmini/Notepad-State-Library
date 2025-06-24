// See https://aka.ms/new-console-template for more information
using NotepadStateLibrary;
using CommandLine;
using System.Diagnostics;
using System.Formats.Asn1;
using System.Globalization;
using System.Text;
using System.ComponentModel;


Parser.Default.ParseArguments<Options>(args)
            .WithParsed(options =>
            {
                string windowStateLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Packages\Microsoft.WindowsNotepad_8wekyb3d8bbwe\LocalState\WindowState");
                List<Guid> guidList = new List<Guid>();


                if (!string.IsNullOrWhiteSpace(options.windowStateLocation))
                {
                    windowStateLocation = options.windowStateLocation;
                }

                foreach (var stringGuid in options.guidTabs ?? new List<string>())
                {
                    if (Guid.TryParse(stringGuid, out Guid guid))
                    {
                        guidList.Add(guid);
                        Console.WriteLine("Add GUID: {0}", stringGuid);
                    }
                    else
                    {
                        Console.WriteLine("Invalid GUID: {0}", stringGuid);
                    }
                }

                Console.WriteLine("********** Starting **********");

                foreach (var path in Directory.EnumerateFiles(windowStateLocation, "*.bin"))
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

                            if (guidList.Count > 0)
                            {
                                np.WriteTabList(guidList);

                                np.ChangeActiveTab(options.activeTab);

                            }

                            

                            o = np.bytes;
                        }
                    }
                    File.WriteAllBytes(path, o);
                }

                Console.WriteLine("********** Finished **********");
            })
            .WithNotParsed(errors =>
            {
                foreach (var error in errors)
                {
                    if (error is HelpRequestedError || error is VersionRequestedError)
                    {

                    }
                    else
                    {
                        Console.WriteLine($"Error: {error}");
                    }
                }
            });


public class Options
{
    [Option('w', "windowstatelocation", Required = false, HelpText = "Window State Folder Location. Default value is the system location.")]
    public string windowStateLocation { get; set; }

    [Option('t', "tabs", Required = true, HelpText = "Space seperated GUIDs to write to Window State File.")]
    public IEnumerable<string> guidTabs { get; set; }

    [Option('a', "activeTab", Required = false, HelpText = "0 based index of the active tab. Default value is 0", Default = 0)]
    public int activeTab { get; set; }
}


