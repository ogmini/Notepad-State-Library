﻿using NotepadStateLibrary;
using CsvHelper;
using System.Globalization;
using CommandLine;

Parser.Default.ParseArguments<Options>(args)
            .WithParsed(options =>
            {
                string tabStateLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Packages\Microsoft.WindowsNotepad_8wekyb3d8bbwe\LocalState\TabState");
                string windowStateLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Packages\Microsoft.WindowsNotepad_8wekyb3d8bbwe\LocalState\WindowState");
                string outputLocation = Directory.GetCurrentDirectory();

                if (!string.IsNullOrWhiteSpace(options.tabStateLocation))
                {
                    tabStateLocation = options.tabStateLocation;
                }
                if (!string.IsNullOrWhiteSpace(options.windowStateLocation))
                {
                    windowStateLocation = options.windowStateLocation;
                }
                if (!string.IsNullOrWhiteSpace(options.outputLocation)) 
                {
                    outputLocation = options.outputLocation;
                }

                Console.WriteLine("********** Starting **********");
                Console.WriteLine("TabState Folder Location - {0}", tabStateLocation);
                if (!Directory.Exists(tabStateLocation)) 
                {
                    Console.WriteLine("!! ERROR: Invalid TabState Folder Location !!");
                    Environment.Exit(3);
                }

                Console.WriteLine("WindowState Folder Location - {0}", windowStateLocation);
                if (!Directory.Exists(windowStateLocation))
                {
                    Console.WriteLine("!! ERROR: Invalid WindowState Folder Location !!");
                    Environment.Exit(3);
                }

                Console.WriteLine("Output Folder Location - {0}", outputLocation);
                if (!Directory.Exists(outputLocation))
                {
                    Console.WriteLine("!! ERROR: Invalid Output Folder Location !!");
                    Environment.Exit(3);
                }

                Console.WriteLine();

                List<NPTabState> fileTabs = new List<NPTabState>();
                List<NPTabState> noFileTabs = new List<NPTabState>();
                List<NPTabState> stateTabs = new List<NPTabState>();
                List<NPWindowState> windowStateTabs = new List<NPWindowState>();

                //Tabstate
                foreach (var path in Directory.EnumerateFiles(tabStateLocation, "*.bin"))
                {
                    byte[] b = File.ReadAllBytes(path);
                    if (b.Length > 0)
                    {
                        Console.WriteLine("Processing TabState - {0}", Path.GetFileName(path));
                        NPTabState np = new NPTabState(b, Path.GetFileName(path));

                        switch (np.TypeFlag)
                        {
                            case 0:
                                noFileTabs.Add(np);
                                break;
                            case 1:
                                fileTabs.Add(np);
                                break;
                            default:
                                stateTabs.Add(np);
                                break;
                        }
                    }
                }

                //Writing File Tabs
                using (var writer = new StreamWriter(Path.Combine(outputLocation, "FileTabs.csv")))
                {
                    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {
                        csv.WriteHeader<NPTabState>();
                        csv.NextRecord();
                        csv.WriteRecords(fileTabs);
                    }
                }

                //Writing No File Tabs
                using (var writer = new StreamWriter(Path.Combine(outputLocation, "NoFileTabs.csv")))
                {
                    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {
                        csv.WriteHeader<NPTabState>();
                        csv.NextRecord();
                        csv.WriteRecords(noFileTabs);
                    }
                }

                //Writing State Tabs
                using (var writer = new StreamWriter(Path.Combine(outputLocation, "StateTabs.csv")))
                {
                    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {
                        csv.WriteHeader<NPTabState>();
                        csv.NextRecord();
                        csv.WriteRecords(stateTabs);
                    }
                }

                //Windowstate
                foreach (var path in Directory.EnumerateFiles(windowStateLocation, "*.bin"))
                {
                    byte[] b = File.ReadAllBytes(path);
                    if (b.Length > 0)
                    {
                        Console.WriteLine("Processing WindowState - {0}", Path.GetFileName(path));
                        NPWindowState np = new NPWindowState(b, Path.GetFileName(path));
                        windowStateTabs.Add(np);
                    }
                }

                //Writing Window States
                using (var writer = new StreamWriter(Path.Combine(outputLocation, "WindowStateTabs.csv")))
                {
                    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {
                        csv.WriteHeader<NPWindowState>();
                        csv.NextRecord();
                        csv.WriteRecords(windowStateTabs);
                    }
                }

                Console.WriteLine("\r\n********** Finished **********");

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
    [Option('t', "tabstatelocation", Required = false, HelpText = "Tab State Folder Location. Default value is the system location.")]
    public string tabStateLocation { get; set; }

    [Option('w', "windowstatelocation", Required = false, HelpText = "Window State Folder Location. Default value is the system location.")]
    public string windowStateLocation { get; set; }

    [Option('o', "outputlocation", Required = false, HelpText = "Output Folder Location for CSV files. Default location is same folder as program.")]
    public string outputLocation { get; set; }


}
