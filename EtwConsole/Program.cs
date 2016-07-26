using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using Microsoft.Diagnostics.Eventing.EventLib;

namespace EtwConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            switch (args.Length)
            {
                case 0:
                    Run();
                    break;

                case 1:
                    string arg = args[0];
                    if (arg.Contains("help", StringComparison.OrdinalIgnoreCase))
                    {
                        PrintHelp();
                    }
                    else if (arg.Contains("pid", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            string[] parts = arg.Split(':');
                            if (parts.Length == 2)
                            {
                                int pid = int.Parse(parts[1]);
                                Run(pid);
                            }
                            else
                            {
                                throw new Exception(String.Format("Invald argument: {0}", arg));
                            }
                        }
                        catch(Exception e)
                        {
                            Console.WriteLine(e.Message);
                            PrintHelp();
                        }
                    }
                    break;

                default:
                    Console.WriteLine("Invald number of arguments.");
                    PrintHelp();
                    break;
            }
            
        }


        private static void PrintHelp()
        {
            Console.WriteLine("Usage: ");
            Console.WriteLine("etwconsole.exe");
            Console.WriteLine("etwconsole.exe /pid:<pid>");
            Console.WriteLine("\t<pid> - Optional - The Process ID to listening to if missing then all processes will be listend too.");
            Console.WriteLine("etwconsole.exe /help");
        }
    }
}
