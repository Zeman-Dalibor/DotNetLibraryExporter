namespace DotNetFrameworkDllExporter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Xml;

    public partial class Namespace
    {
        private class NamespaceXmlPrinter
        {
            public static void PrintType(XmlWriter writer, Type type)
            {
                if (type.IsClass)
                {
                    writer.WriteStartElement("Class");
                }
                else if (type.IsInterface)
                {
                    writer.WriteStartElement("Interface");
                }
                else if (type.IsValueType)
                {
                    writer.WriteStartElement("ValueType");
                }
                else if (type.IsEnum)
                {
                    writer.WriteStartElement("Enum");
                }

                writer.WriteAttributeString("name", type.Name);

                // Fields
                foreach (FieldInfo fieldInfo in type.GetFields())
                {
                    writer.WriteStartElement("Field");
                    writer.WriteAttributeString("name", fieldInfo.Name);
                    writer.WriteAttributeString("static", fieldInfo.IsStatic.ToString());
                    writer.WriteAttributeString("type", GetCSharpFullName(fieldInfo.FieldType));
                    writer.WriteEndElement();
                }

                // Properties
                foreach (PropertyInfo propertyInfo in type.GetProperties())
                {
                    writer.WriteStartElement("Property");
                    writer.WriteAttributeString("name", propertyInfo.Name);
                    writer.WriteAttributeString("type", GetCSharpFullName(propertyInfo.PropertyType));
                    writer.WriteEndElement();
                }

                IEnumerable<string> propertiesNames = type.GetProperties().Select(property => property.Name);

                // Methods
                foreach (MethodInfo methodInfo in type.GetMethods())
                {
                    // Skip auto-generated methods from properties
                    if (methodInfo.Name.StartsWith("set_") || methodInfo.Name.StartsWith("get_"))
                    {
                        string potentialPropertyName = methodInfo.Name.Substring(4);
                        if (propertiesNames.Contains(potentialPropertyName))
                        {
                            continue;
                        }
                    }

                    writer.WriteStartElement("Method");
                    writer.WriteAttributeString("name", methodInfo.Name);
                    writer.WriteAttributeString("static", methodInfo.IsStatic.ToString());
                    writer.WriteAttributeString("return", GetCSharpFullName(methodInfo.ReturnType));
                    foreach (ParameterInfo parameter in methodInfo.GetParameters())
                    {
                        writer.WriteStartElement("Parameter");
                        writer.WriteAttributeString("name", parameter.Name);
                        writer.WriteAttributeString("type", GetCSharpFullName(parameter.ParameterType));
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                }

                Type[] nestedTypes = type.GetNestedTypes();
                if (nestedTypes.Length != 0)
                {
                    writer.WriteStartElement("NestedTypes");

                    foreach (Type nestedType in nestedTypes)
                    {
                        PrintType(writer, nestedType);
                    }

                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
            }

            private static string GetCSharpFullName(Type type)
            {
                if (!type.IsConstructedGenericType)
                {
                    return type.FullName;
                }

                Type[] genericTypes = type.GetGenericArguments();
                string genericTypesString = string.Join(", ", genericTypes.Select(t => GetCSharpFullName(t)));

                return type.Namespace + "." + type.Name.Substring(0, type.Name.IndexOf("`")) + "[" + genericTypesString + "]";
            }
        }
    }
}