namespace DotNetFrameworkDllExporter
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Xml;
    using DotNetFrameworkDllExporter.XmlPrinter;

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

        internal void PrintXml(XmlWriter writer, EntityIdPrinter idPrinter)
        {
            writer.WriteStartElement("Model");
            idPrinter.PrintStartElementEntityId(this.Name, EntityIdPrinter.ElementType.Module);
            writer.WriteAttributeString("name", this.Name);

            foreach (Type type in this.Types)
            {
                writer.WriteStartElement("Namespace");
                idPrinter.PrintStartElementEntityId(type.Name, EntityIdPrinter.ElementType.Concept);

                writer.WriteAttributeString("name", this.Name);
                NamespaceXmlPrinter.PrintType(writer, type, idPrinter);

                idPrinter.LeaveElement();
                writer.WriteEndElement();
            }

            foreach (KeyValuePair<string, Namespace> nameSpace in this.InnerNamespaces)
            {
                nameSpace.Value.PrintXml(writer, idPrinter);
            }

            idPrinter.LeaveElement();
            writer.WriteEndElement();
        }
    }
}