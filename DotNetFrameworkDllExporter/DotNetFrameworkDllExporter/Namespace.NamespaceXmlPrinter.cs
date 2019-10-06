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
                    if (type.IsSubclassOf(typeof(MulticastDelegate)))
                    {
                        PrintDelegate(writer, type);
                        return;
                    }
                    else
                    {
                        writer.WriteStartElement("Class");
                    }
                }
                else if (type.IsInterface)
                {
                    writer.WriteStartElement("Interface");
                }
                else if (type.IsValueType)
                {
                    if (type.IsEnum) // Enum is also ValueType It must be checked first
                    {
                        PrintEnum(writer, type);
                        return;
                    }
                    else
                    {
                        writer.WriteStartElement("Struct");
                    }
                }
                else
                {
                    throw new NotSupportedException();
                }

                writer.WriteAttributeString("name", SimplifiedName(type));

                // Generic parameters
                foreach (Type genericParam in type.GetGenericArguments())
                {
                    writer.WriteStartElement("GenericParameter");
                    writer.WriteAttributeString("name", genericParam.Name);
                    writer.WriteEndElement();
                }

                // Inheritance
                if (type.BaseType != null)
                {
                    writer.WriteStartElement("BaseClass");
                    writer.WriteAttributeString("name", GetCSharpFullName(type.BaseType));
                    writer.WriteEndElement();
                }

                // Interface implement
                foreach (var interfaceType in type.GetInterfaces())
                {
                    writer.WriteStartElement("InterfaceImplemented");
                    writer.WriteAttributeString("name", GetCSharpFullName(interfaceType));
                    writer.WriteEndElement();
                }

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
                    writer.WriteAttributeString("get", (propertyInfo.GetMethod != null).ToString());
                    writer.WriteAttributeString("set", (propertyInfo.SetMethod != null).ToString());
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
                    
                    // Generic parameters
                    foreach (Type genericParam in methodInfo.GetGenericArguments())
                    {
                        writer.WriteStartElement("GenericParameter");
                        writer.WriteAttributeString("name", genericParam.Name);
                        writer.WriteEndElement();
                    }

                    foreach (ParameterInfo parameter in methodInfo.GetParameters())
                    {
                        writer.WriteStartElement("Parameter");
                        writer.WriteAttributeString("name", parameter.Name);
                        writer.WriteAttributeString("type", GetCSharpFullName(parameter.ParameterType));
                        writer.WriteAttributeString("ref", (parameter.ParameterType.IsByRef && !parameter.IsOut).ToString());
                        writer.WriteAttributeString("out", parameter.IsOut.ToString());
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                }

                foreach (var constructorInfo in type.GetConstructors())
                {
                    writer.WriteStartElement("Constructor");

                    foreach (ParameterInfo parameter in constructorInfo.GetParameters())
                    {
                        writer.WriteStartElement("Parameter");
                        writer.WriteAttributeString("name", parameter.Name);
                        writer.WriteAttributeString("type", GetCSharpFullName(parameter.ParameterType));
                        writer.WriteAttributeString("ref", (parameter.ParameterType.IsByRef && !parameter.IsOut).ToString());
                        writer.WriteAttributeString("out", parameter.IsOut.ToString());
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                }

                // Nested types
                foreach (Type nestedType in type.GetNestedTypes())
                {
                    PrintType(writer, nestedType);
                }

                writer.WriteEndElement();
            }

            private static void PrintEnum(XmlWriter writer, Type type)
            {
                writer.WriteStartElement("Enum");
                foreach (var enumName in type.GetEnumNames())
                {
                    writer.WriteStartElement("EnumName");
                    writer.WriteAttributeString("name", enumName);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }

            private static void PrintDelegate(XmlWriter writer, Type type)
            {
                writer.WriteStartElement("Delegate");
                writer.WriteAttributeString("name", SimplifiedName(type));

                var methodInfo = type.GetMethod("Invoke");
                writer.WriteAttributeString("return", GetCSharpFullName(methodInfo.ReturnType));

                // Generic parameters
                foreach (Type genericParam in type.GetGenericArguments())
                {
                    writer.WriteStartElement("GenericParameter");
                    writer.WriteAttributeString("name", genericParam.Name);
                    writer.WriteEndElement();
                }

                foreach (ParameterInfo parameter in methodInfo.GetParameters())
                {
                    writer.WriteStartElement("Parameter");
                    writer.WriteAttributeString("name", parameter.Name);
                    writer.WriteAttributeString("type", GetCSharpFullName(parameter.ParameterType));
                    writer.WriteAttributeString("ref", (parameter.ParameterType.IsByRef && !parameter.IsOut).ToString());
                    writer.WriteAttributeString("out", parameter.IsOut.ToString());
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
            }

            private static string SimplifiedName(Type type)
            {
                return type.Name.Split(new[] { '`' }, 2)[0];
            }

            private static string GetCSharpFullName(Type type)
            {
                if (type.IsByRef)
                {
                    return GetCSharpFullName(type.GetElementType());
                }
                else if (type.IsPointer)
                {
                    return $"{GetCSharpFullName(type.GetElementType())}*";
                }
                else if (type.IsArray)
                {
                    return $"{GetCSharpFullName(type.GetElementType())}[]";
                }
                else if (type.IsConstructedGenericType || type.IsGenericType)
                {
                    return GetGenericTypeFormat(type);
                }
                else if (type.IsGenericParameter)
                {
                    return type.Name;
                }
                else if (type.IsNested)
                {
                    return $"{GetCSharpFullName(type.DeclaringType)}.{type.Name}";
                }
                else
                {
                    return type.FullName;
                }
            }

            private static string GetGenericTypeFormat(Type type)
            {
                Type[] genericTypes = type.GetGenericArguments();
                return GetReformatGeneric(type, genericTypes, genericTypes.Length);
            }

            private static string GetReformatGeneric(Type type, Type[] genericTypes, int length)
            {
                ParseNameAndGenericParameterCount(type, out string name, out int genericParamsCount);

                var genericParams = genericTypes.Take(length).Skip(length - genericParamsCount).ToList();
                string genericTypesString = string.Join(", ", genericParams.Select(t => GetCSharpFullName(t)));

                if (type.IsNested)
                {
                    string genericAppendix = string.IsNullOrWhiteSpace(genericTypesString) ? string.Empty : "{" + genericTypesString + "}";

                    return GetReformatGeneric(type.DeclaringType, genericTypes, length - genericParamsCount) + "." + name + genericAppendix;
                }
                else
                {
                    if (genericParamsCount != length)
                    {
                        throw new InvalidOperationException();
                    }

                    string genericAppendix = string.IsNullOrWhiteSpace(genericTypesString) ? string.Empty : "{" + genericTypesString + "}";

                    return type.Namespace + "." + name + genericAppendix;
                }
            }

            private static void ParseNameAndGenericParameterCount(Type type, out string name, out int numberOfGenericParams)
            {
                if (!type.Name.Contains('`'))
                {
                    name = type.Name;
                    numberOfGenericParams = 0;
                }
                else
                {
                    string[] tokens = type.Name.Split('`');
                    name = tokens[0];
                    numberOfGenericParams = int.Parse(tokens[1]);
                }
            }
        }
    }
}