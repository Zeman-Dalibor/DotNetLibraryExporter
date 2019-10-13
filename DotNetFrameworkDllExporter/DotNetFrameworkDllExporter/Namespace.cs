namespace DotNetFrameworkDllExporter
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Xml;

    public partial class Namespace
    {
        public Namespace(string nameSpaceName, Namespace parent)
        {
            this.Name = nameSpaceName;
            this.ParentNamespace = parent;
        }

        public string Name { get; private set; } = null;

        public Namespace ParentNamespace { get; private set; } = null;

        public Dictionary<string, Namespace> InnerNamespaces { get; private set; } = new Dictionary<string, Namespace>();

        public List<Type> Types { get; private set; } = new List<Type>();

        public string GetFullNamespace()
        {
            Namespace handle = this;
            var names = new List<string>();
            while (handle.ParentNamespace != null)
            {
                names.Add(handle.Name);
                handle = handle.ParentNamespace;
            }

            names.Reverse();

            return string.Join(".", names);
        }

        internal static Namespace Create(Assembly assembly)
        {
            return NamespaceBuilder.Build(assembly);
        }

        internal void PrintXml(XmlWriter writer)
        {
            writer.WriteStartElement("Namespace");
            writer.WriteAttributeString("entityId", this.GetFullNamespace());
            writer.WriteAttributeString("name", this.Name);

            foreach (Type type in this.Types)
            {
                NamespaceXmlPrinter.PrintType(writer, type);
            }

            foreach (KeyValuePair<string, Namespace> nameSpace in this.InnerNamespaces)
            {
                nameSpace.Value.PrintXml(writer);
            }

            writer.WriteEndElement();
        }
    }
}