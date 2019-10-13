namespace DotNetFrameworkDllExporter
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Xml;

    internal class DLLExporter
    {
        private readonly TextWriter output;

        public DLLExporter(TextWriter output)
        {
            this.output = output;
        }

        internal void ExportAPI(string dllFileName, string alias = null)
        {
            Assembly sampleAssembly = GetAssemblyByPath(dllFileName);

            var globalNamespace = this.BuildNamespaceTree(sampleAssembly);

            this.PrintToXml(globalNamespace, alias);
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

        private Namespace BuildNamespaceTree(Assembly assembly)
        {
            return Namespace.Create(assembly);
        }
        
        private void PrintToXml(Namespace globalNamespace, string alias = null)
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
                if (alias != null)
                {
                    writer.WriteAttributeString("Alias", alias);
                }

                foreach (var nameSpace in globalNamespace.InnerNamespaces.Values)
                {
                    nameSpace.PrintXml(writer);
                }

                writer.WriteEndElement();
                writer.Flush();
            }
        }
    }
}