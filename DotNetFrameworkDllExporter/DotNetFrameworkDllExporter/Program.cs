namespace DotNetFrameworkDllExporter
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Text;

    internal class Program
    {
        public static bool WriteEntityId = true;

        public static List<Type> ExcludedTypes = new List<Type>();

        private static string dllFileName = null;
        private static bool interactive = false;
        private static string outputFilename = null;

        private static void Main(string[] args)
        {
            Start(args, Console.In, Console.Out);

            if (interactive)
            {
                if (outputFilename != null)
                {
                    Console.WriteLine("Extraction done! Press any key to exit program.");
                }

                Console.ReadKey();
            }
        }

        private static void Start(string[] args, TextReader input, TextWriter output)
        {
            if (TryProcessStartupArguments(args, output))
            {
                if (outputFilename != null)
                {
                    using (var sw = new StreamWriter(outputFilename, false, Encoding.UTF8))
                    {
                        var dllExporter = new DLLExporter(sw);
                        dllExporter.ExportAPI(dllFileName);
                    }
                }
                else
                {
                    var dllExporter = new DLLExporter(output);
                    dllExporter.ExportAPI(dllFileName);
                }
            }
            else
            {
                return;
            }
        }

        private static bool TryProcessStartupArguments(string[] args, TextWriter output)
        {
            if (args.Length == 0)
            {
                PrintHelp(output);
                return false;
            }

            int skipArguments = 0;

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                if (skipArguments > 0)
                {
                    skipArguments--;
                    continue;
                }

                if (arg.StartsWith("-", true, CultureInfo.CurrentCulture))
                {
                    skipArguments = ProcessNonPositionalParameters(args, i);
                }
                else
                {
                    ProcessPositionalParameters(arg);
                }
            }

            if (dllFileName == null || !File.Exists(dllFileName))
            {
                output.WriteLine("Path to the dll file is not valid.");
                return false;
            }

            return true;
        }

        private static void PrintHelp(TextWriter output)
        {
            output.WriteLine($".NET Framework Dll Exporter {Assembly.GetExecutingAssembly().GetName().Version.ToString()}");
            output.WriteLine("Utility for extracting API from .NET Framework DLL");
            output.WriteLine(" ------------------------------------------------ ");
            output.WriteLine($"Usage: {Path.GetFileName(Assembly.GetEntryAssembly().Location)} <path-to-extracting-dll-file> [-o <path-to-output-file>]");
            output.WriteLine("-i - interactive mode");
            output.WriteLine("-o <output-file-path> - specify output file");
            output.WriteLine("-e - disable entity ID");
            output.WriteLine("-b <blacklist-file-path> - specify blacklist file");
            output.WriteLine();
            Console.ReadKey();
        }

        private static void ProcessPositionalParameters(string arg)
        {
            if (dllFileName == null)
            {
                dllFileName = arg;
            }
        }

        private static int ProcessNonPositionalParameters(string[] args, int i)
        {
            int skipArguments = 0;
            switch (args[i])
            {
                case "-i":
                    interactive = true;
                    break;

                case "-o":
                    outputFilename = args[i + 1];
                    skipArguments = 1;
                    break;

                case "-e":
                    WriteEntityId = false;
                    break;

                case "-b":
                    ExcludedTypes = LoadBlacklistFile(args[i + 1]);
                    skipArguments = 1;
                    break;
            }

            return skipArguments;
        }

        private static List<Type> LoadBlacklistFile(string v)
        {
            var blacklist = new List<Type>();
            using (var sr = new StreamReader(v))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    blacklist.Add(Type.GetType(line));
                }
            }

            return blacklist;
        }
    }
}
