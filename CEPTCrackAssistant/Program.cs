using CEPTCrackAssistant.Properties;

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using Utilities;

/* TODO:
 *  - Detect the parent process (CMD or Explorer) and conditionally show "Done." message.
 */

namespace CEPTCrackAssistant
{
    class Program
    {
        static void Main(string[] args)
        {
            OrderedDictionary odPrograms = new OrderedDictionary();

            odPrograms.Add("BulkDnl", Resources.BulkDnl);
            odPrograms.Add("Comparator", Resources.Comparator);
            odPrograms.Add("context", Resources.context);
            odPrograms.Add("Control Panel", Resources.Control_Panel);
            odPrograms.Add("FDF", Resources.FDF);
            odPrograms.Add("File Type Doctor", Resources.File_Type_Doctor);
            odPrograms.Add("poweren", Resources.poweren);
            odPrograms.Add("RegAgent", Resources.RegAgent);
            odPrograms.Add("Startup", Resources.Startup);
            
            string sTitle = ApplicationInfo.Title,
                   sProgFilesDir = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                   sX86ProgFilesDir = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                   sSuffix = @"\Creative Element Power Tools",
                   sPath;

            List<string> lArgs = new List<string>(args);

            ConsoleSpinner spinner = new ConsoleSpinner();
            
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken ct = tokenSource.Token;

            // Constants and constant-like strings.
            string TEXT_USAGE = "Copies cracked versions of the Power Tools executables to the\n" +
                                "installation directory.\n\n" +
                                "CEPTCA [/?] [/u [...]]\n\n" +
                                "\t/?\t\tDisplays this help message.\n" +
                                "\t/u [...]\tUncracks the programs.",

                   TEXT_WELCOME = String.Format("Welcome to the {0}!\n" +
                                                "This will automatically copy cracked versions of the Power\n"+
                                                "Tools executables to the installation directory.\n\n", sTitle);

            // Set the foreground to green, the font to some tiny one, and the icon to the program's.
            Console.ForegroundColor = ConsoleColor.Green;
            //ConsoleHelper.SetConsoleFont(4);
            ConsoleHelper.SetConsoleIcon(Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location));
            
            Console.Title = sTitle + " v" + ApplicationInfo.Version.ToString();

            if (lArgs.Count >= 1 && lArgs[0].Equals("/?"))
            {
                Console.Write(TEXT_USAGE);
                goto end;
            }

            Console.Write(TEXT_WELCOME);
            ConsoleHelper.Pause();
            Console.Write("\n\n");

            if (Directory.Exists(sProgFilesDir + sSuffix))
            {
                sPath = sProgFilesDir + sSuffix;
            }
            else if (Directory.Exists(sX86ProgFilesDir + sSuffix))
            {
                sPath = sX86ProgFilesDir + sSuffix;
            }
            else
            {
                Console.Write("Installation directory not found.");
                goto pause;
            }

            DirectoryInfo diInstall = new DirectoryInfo(sPath);

            // Handle command-line arguments.
            if (lArgs.Count >= 1)
            {
                string sCommand = lArgs[0].ToLower();
                switch (sCommand)
                {
                    case "/u":
                        // If /u is specified, assign the original files to the byte arrays.
                        if (sCommand.Equals("/u"))
                        {
                            odPrograms["BulkDnl"] = Resources.BulkDnl_orig;
                            odPrograms["Comparator"] = Resources.Comparator_orig;
                            odPrograms["context"] = Resources.context_orig;
                            odPrograms["Control Panel"] = Resources.Control_Panel_orig;
                            odPrograms["FDF"] = Resources.FDF_orig;
                            odPrograms["File Type Doctor"] = Resources.File_Type_Doctor_orig;
                            odPrograms["poweren"] = Resources.poweren_orig;
                            odPrograms["RegAgent"] = Resources.RegAgent_orig;
                            odPrograms["Startup"] = Resources.Startup_orig;
                        }

                        if (lArgs.Count >= 1 && lArgs[0].Equals("/u"))
                            Console.Write("Uncracking...\n\n");
                        break;
                }
            }

            cracking:

            // If the folder exists (it has to—it is found), it copies the cracked programs.
            if (diInstall.Exists)
            {
                string[] oKeys = new string[odPrograms.Keys.Count];
                odPrograms.Keys.CopyTo(oKeys, 0);

                for (int j = 0; j < odPrograms.Keys.Count; j++)
                {
                    byte[] bProgram = (byte[])odPrograms[j];
                    string sKey = oKeys[j],
                           sProgPath = String.Format(@"{0}\{1}.exe", sPath, sKey);

                    // Check if the file is being used by another process.
                    if (!FileHelper.IsLocked(sProgPath))
                    {
                        Console.WriteLine("Overwriting \"{0}\"...", sProgPath);
                        File.WriteAllBytes(sProgPath, bProgram);
                    }
                    else
                    {
                        Console.WriteLine("\"{0}\" is being used by another process. Could not copy.", sPath);
                    }
                }
            }
            else
            {
                // This should never happen.
                Console.WriteLine("\"{0}\" does not exist.", sPath);
            }

            pause:
            Console.Write("\nDone.");
            Console.ReadKey(true);

            end:
            Console.WriteLine();
            Application.Exit();
        }
    }
}
