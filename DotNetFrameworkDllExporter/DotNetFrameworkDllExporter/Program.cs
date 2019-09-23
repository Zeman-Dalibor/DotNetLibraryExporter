namespace DotNetFrameworkDllExporter
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Text;

    internal class Program
    {
        /*
         * "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.6.1\System.dll";
         * "DotNetFrameworkExampleDll.dll"
         *
         */
        private static string dllFileName = null;
        private static bool interactive = false;
        private static string outputFilename = null;

        private static void Main(string[] args)
        {
            Start(args, Console.In, Console.Out);

            if (interactive)
            {
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
            }

            return skipArguments;
        }
    }
}
