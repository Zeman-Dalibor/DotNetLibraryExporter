namespace DotNetFrameworkDllExporter.XmlPrinter
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xml;

    internal class EntityIdPrinter
    {
        public enum ElementType
        {
            Module,
            Concept,
        }

        private readonly List<(string id, ElementType type)> entityIdStack = new List<(string, ElementType)>();
        private readonly XmlWriter writer;

        public EntityIdPrinter(XmlWriter writer)
        {
            this.writer = writer;
        }

        public void PrintStartElementEntityId(string append, ElementType type)
        {
            this.AddElement(append, type);
            this.PrintStack();
        }

        public void LeaveElement() => this.entityIdStack.RemoveAt(this.entityIdStack.Count - 1);

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
