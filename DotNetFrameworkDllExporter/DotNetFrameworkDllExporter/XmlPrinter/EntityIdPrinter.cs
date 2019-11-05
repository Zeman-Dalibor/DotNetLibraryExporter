namespace DotNetFrameworkDllExporter.XmlPrinter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xml;

    internal class EntityIdPrinter
    {
        internal enum ElementType
        {
            Module,
            Concept,
        }

        private readonly List<(string id, ElementType type)> entityIdStack = new List<(string, ElementType)>();
        private readonly XmlWriter writer;

        internal EntityIdPrinter(XmlWriter writer)
        {
            this.writer = writer;
        }

        internal void AddEntityId(string append, ElementType type)
        {
            this.AddElement(append, type);
        }

        internal void PrintStartElementEntityId(string append, ElementType type)
        {
            this.AddElement(append, type);
            this.PrintStack();
        }

        internal void LeaveElement() => this.entityIdStack.RemoveAt(this.entityIdStack.Count - 1);

        internal void RemoveEntityId() => this.entityIdStack.RemoveAt(this.entityIdStack.Count - 1);

        private void PrintStack()
        {
            var sb = new StringBuilder();

            var (firstId, firstType) = this.entityIdStack.First();
            sb.Append(firstId);
            ElementType lastType = firstType;

            foreach (var (id, type) in this.entityIdStack.Skip(1))
            {
                if (lastType != type)
                {
                    sb.Append(":");
                }
                else
                {
                    sb.Append(".");
                }

                sb.Append(id);
                lastType = type;
            }

            this.writer.WriteAttributeString("entityId", sb.ToString());
        }

        private void AddElement(string append, ElementType type) => this.entityIdStack.Add((append, type));
    }
}
