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

        private readonly string dtdStr = @"
<!ELEMENT Assembly (Namespace|Interface|Class|Enum|Struct)*>
<!ELEMENT Namespace (Namespace|Interface|Class|Enum|Struct)*>
    <!ATTLIST Namespace name CDATA #REQUIRED>

<!ELEMENT Interface (Method|Property)*>
    <!ATTLIST Interface name CDATA #REQUIRED>

<!ELEMENT Enum (#PCDATA)>
    <!ATTLIST Enum name CDATA #REQUIRED>

<!ELEMENT Class (Namespace|Interface|Class|Enum|Struct|Field|Property|Method|Constructor)*>
    <!ATTLIST Class name CDATA #REQUIRED>
    <!ELEMENT GenericParameter (#PCDATA)>
        <!ATTLIST GenericParameter name CDATA #REQUIRED>

<!ELEMENT Delegate (GenericParameter|Parameter)*>
    <!ATTLIST Delegate name CDATA #REQUIRED>
    <!ATTLIST Delegate return CDATA #REQUIRED>

<!ELEMENT Struct (Namespace|Interface|Class|Enum|Struct|Field|Property|Method|Constructor)*>
    <!ATTLIST Struct name CDATA #REQUIRED>

<!ELEMENT Field (#PCDATA)>
    <!ATTLIST Field name CDATA #REQUIRED>
    <!ATTLIST Field static CDATA #REQUIRED>
    <!ATTLIST Field type CDATA #REQUIRED>

<!ELEMENT Property (#PCDATA)>
    <!ATTLIST Property name CDATA #REQUIRED>
    <!ATTLIST Property type CDATA #REQUIRED>
    <!ATTLIST Property set CDATA #REQUIRED>
    <!ATTLIST Property get CDATA #REQUIRED>

<!ELEMENT Method (GenericParameter|Parameter)*>
    <!ATTLIST Method name CDATA #REQUIRED>
    <!ATTLIST Method static CDATA #REQUIRED>
    <!ATTLIST Method return CDATA #REQUIRED>
    <!ELEMENT Parameter (#PCDATA)>
        <!ATTLIST Parameter name CDATA #REQUIRED>
        <!ATTLIST Parameter type CDATA #REQUIRED>
        <!ATTLIST Parameter ref CDATA #REQUIRED>
        <!ATTLIST Parameter out CDATA #REQUIRED>

<!ELEMENT Constructor (Parameter)*>
";

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
                writer.WriteDocType("Assembly", null, null, this.dtdStr);
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