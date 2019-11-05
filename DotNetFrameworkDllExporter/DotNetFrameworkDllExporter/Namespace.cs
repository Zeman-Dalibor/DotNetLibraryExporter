namespace DotNetFrameworkDllExporter
{
    using System;
    using System.Collections.Generic;

    public class Namespace
    {
        public Namespace(string name, Namespace parent)
        {
            this.Name = name;
            this.ParentNamespace = parent;
        }

        public string Name { get; } = null;

        public Namespace ParentNamespace { get; } = null;

        public Dictionary<string, Namespace> InnerNamespaces { get; } = new Dictionary<string, Namespace>();

        public List<Type> Types { get; } = new List<Type>();

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
    }
}