namespace DotNetFrameworkDllExporter
{
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Xml;
    using DotNetFrameworkDllExporter.XmlPrinter;

    internal class DLLExporter
    {
        private readonly TextWriter output;

        public DLLExporter(TextWriter output)
        {
            this.output = output;
        }

        internal void ExportAPI(params string[] dllFileName)
        {
            var globalNamespace = this.BuildNamespaceTree(dllFileName);

            this.PrintToXml(globalNamespace);
        }

        private static Assembly GetAssemblyByPath(string dllFileName)
        {
            string absolutePath = dllFileName;
            if (!Path.IsPathRooted(dllFileName))
            {
                absolutePath = Path.Combine(Directory.GetCurrentDirectory(), dllFileName);
            }

            var assembly = Assembly.LoadFile(absolutePath);
            return assembly;
        }

        private Namespace BuildNamespaceTree(params string[] dllFileName)
        {
            Assembly assembly = GetAssemblyByPath(dllFileName[0]);

            return NamespaceBuilder.Build(assembly);
        }
        
        private void PrintToXml(Namespace globalNamespace)
        {
            var settings = new XmlWriterSettings
            {
                Indent = true,
                WriteEndDocumentOnClose = true,
                Encoding = Encoding.UTF8,
            };

            using (var writer = XmlWriter.Create(this.output, settings))
            {
                writer.WriteStartDocument();
                writer.WriteDocType("Assembly", null, null, MainResources.AssemblyDtd);
                writer.WriteStartElement("Assembly");

                var entityIdPrinter = new EntityIdPrinter(writer);
                var namespaceXmlPrinter = new NamespaceXmlPrinter(writer, entityIdPrinter);
                foreach (var nameSpace in globalNamespace.InnerNamespaces.Values)
                {
                    namespaceXmlPrinter.PrintNamespace(nameSpace);
                }

                writer.WriteEndElement();
                writer.Flush();
            }
        }
    }
}