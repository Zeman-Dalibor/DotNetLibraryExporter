namespace DotNetFrameworkDllExporter
{
    using System;
    using System.Reflection;

    internal static class NamespaceBuilder
    {
        public static Namespace Build(Assembly assembly)
        {
            var global = new Namespace(null, null);
            foreach (Type type in assembly.GetTypes())
            {
                AddType(global, type);
            }

            return global;
        }

        private static void AddType(Namespace global, Type type)
        {
            if (type.IsNested)
            {
                return;
            }

            if (type.Namespace == null)
            {
                Console.WriteLine($"WARN: type {type} has no namespace.");
                global.Types.Add(type);
                return;
            }

            Namespace handle = global;
            string[] tokens = type.Namespace.Split('.');

            foreach (string name in tokens)
            {
                handle = AddOrGetNamespace(handle, name);
            }

            handle.Types.Add(type);
        }

        private static Namespace AddOrGetNamespace(Namespace nameSpace, string name)
        {
            if (!nameSpace.InnerNamespaces.ContainsKey(name))
            {
                nameSpace.InnerNamespaces.Add(name, new Namespace(name, nameSpace));
            }

            return nameSpace.InnerNamespaces[name];
        }
    }
}