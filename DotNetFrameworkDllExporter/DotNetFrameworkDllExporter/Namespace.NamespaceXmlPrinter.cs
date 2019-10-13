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
                        PrintClass(writer, type);
                        return;
                    }
                }
                else if (type.IsInterface)
                {
                    PrintInterface(writer, type);
                    return;
                }
                else if (type.IsValueType)
                {
                    // Enum is also ValueType It must be checked first
                    if (type.IsEnum)
                    {
                        PrintEnum(writer, type);
                        return;
                    }
                    else
                    {
                        PrintStruct(writer, type);
                        return;
                    }
                }
                else
                {
                    throw new NotSupportedException();
                }
            }

            private static void PrintEntityIdOfType(XmlWriter writer, Type type)
            {
                if (Program.WriteEntityId)
                {
                    writer.WriteAttributeString("entityId", $"{type.Namespace}.{type.Name}");
                }
            }

            private static void PrintInheritanceOfType(XmlWriter writer, Type type)
            {
                // Inheritance
                if (type.BaseType != null)
                {
                    writer.WriteAttributeString("BaseClass", GetCSharpFullName(type.BaseType));
                }

                // Interface implement
                if (type.GetInterfaces().Length > 0)
                {
                    writer.WriteAttributeString("InterfaceImplemented", string.Join(";", type.GetInterfaces().Select(interfaceType => GetCSharpFullName(interfaceType))));
                }
            }

            private static void PrintConstructorsOfType(XmlWriter writer, Type type)
            {
                foreach (var constructorInfo in type.GetConstructors())
                {
                    writer.WriteStartElement("Constructor");
                    string methodParamsString = string.Join(",", constructorInfo.GetParameters().Select(mt => GetCSharpFullName(mt.ParameterType)));
                    if (Program.WriteEntityId)
                    {
                        writer.WriteAttributeString("entityId", $"{type.Namespace}.{type.Name}.{constructorInfo.Name}({methodParamsString})");
                    }

                    foreach (ParameterInfo parameter in constructorInfo.GetParameters())
                    {
                        writer.WriteStartElement("Parameter");
                        if (Program.WriteEntityId)
                        {
                            writer.WriteAttributeString("entityId", $"{type.Namespace}.{type.Name}.{constructorInfo.Name}({methodParamsString}).{parameter.Name}");
                        }

                        writer.WriteAttributeString("name", parameter.Name);
                        writer.WriteAttributeString("type", GetCSharpFullName(parameter.ParameterType));
                        writer.WriteAttributeString("ref", (parameter.ParameterType.IsByRef && !parameter.IsOut).ToString());
                        writer.WriteAttributeString("out", parameter.IsOut.ToString());
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                }
            }

            private static void PrintMethodsOfType(XmlWriter writer, Type type)
            {
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
                    string genericParamsString = string.Join(",", methodInfo.GetGenericArguments().Select(mt => GetCSharpFullName(mt)));
                    string methodParamsString = string.Join(",", methodInfo.GetParameters().Select(mt => GetCSharpFullName(mt.ParameterType)));
                    if (Program.WriteEntityId)
                    {
                        writer.WriteAttributeString("entityId", $"{type.Namespace}.{type.Name}.{methodInfo.Name}{{{genericParamsString}}}({methodParamsString})");
                    }

                    writer.WriteAttributeString("name", methodInfo.Name);
                    writer.WriteAttributeString("static", methodInfo.IsStatic.ToString());
                    writer.WriteAttributeString("return", GetCSharpFullName(methodInfo.ReturnType));

                    // Generic parameters
                    foreach (Type genericParam in methodInfo.GetGenericArguments())
                    {
                        writer.WriteStartElement("GenericParameter");
                        if (Program.WriteEntityId)
                        {
                            writer.WriteAttributeString("entityId", $"{type.Namespace}.{type.Name}.{methodInfo.Name}{{{genericParamsString}}}({methodParamsString}).#{genericParam.Name}");
                        }

                        writer.WriteAttributeString("name", genericParam.Name);
                        writer.WriteEndElement();
                    }

                    foreach (ParameterInfo parameter in methodInfo.GetParameters())
                    {
                        writer.WriteStartElement("Parameter");
                        if (Program.WriteEntityId)
                        {
                            writer.WriteAttributeString("entityId", $"{type.Namespace}.{type.Name}.{methodInfo.Name}{{{genericParamsString}}}({methodParamsString}).{parameter.Name}");
                        }

                        writer.WriteAttributeString("name", parameter.Name);
                        writer.WriteAttributeString("type", GetCSharpFullName(parameter.ParameterType));
                        writer.WriteAttributeString("ref", (parameter.ParameterType.IsByRef && !parameter.IsOut).ToString());
                        writer.WriteAttributeString("out", parameter.IsOut.ToString());
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                }
            }

            private static void PrintPropertiesOfType(XmlWriter writer, Type type)
            {
                // Properties
                foreach (PropertyInfo propertyInfo in type.GetProperties())
                {
                    writer.WriteStartElement("Property");
                    if (Program.WriteEntityId)
                    {
                        writer.WriteAttributeString("entityId", $"{type.Namespace}.{type.Name}.{propertyInfo.Name}");
                    }

                    writer.WriteAttributeString("name", propertyInfo.Name);
                    writer.WriteAttributeString("type", GetCSharpFullName(propertyInfo.PropertyType));
                    writer.WriteAttributeString("get", (propertyInfo.GetMethod != null).ToString());
                    writer.WriteAttributeString("set", (propertyInfo.SetMethod != null).ToString());
                    writer.WriteEndElement();
                }
            }

            private static void PrintFieldsOfType(XmlWriter writer, Type type)
            {
                // Fields
                foreach (FieldInfo fieldInfo in type.GetFields())
                {
                    writer.WriteStartElement("Field");
                    if (Program.WriteEntityId)
                    {
                        writer.WriteAttributeString("entityId", $"{type.Namespace}.{type.Name}.{fieldInfo.Name}");
                    }

                    writer.WriteAttributeString("name", fieldInfo.Name);
                    writer.WriteAttributeString("static", fieldInfo.IsStatic.ToString());
                    writer.WriteAttributeString("type", GetCSharpFullName(fieldInfo.FieldType));
                    writer.WriteEndElement();
                }
            }

            private static void PrintGenericParametersOfType(XmlWriter writer, Type type)
            {
                // Generic parameters
                foreach (Type genericParam in type.GetGenericArguments())
                {
                    writer.WriteStartElement("GenericParameter");
                    if (Program.WriteEntityId)
                    {
                        writer.WriteAttributeString("entityId", $"{type.Namespace}.{type.Name}.{genericParam.Name}");
                    }

                    writer.WriteAttributeString("name", genericParam.Name);
                    writer.WriteEndElement();
                }
            }

            private static void PrintInterface(XmlWriter writer, Type type)
            {
                writer.WriteStartElement("Interface");
                PrintEntityIdOfType(writer, type);

                writer.WriteAttributeString("name", GetNameWithoutGenericMark(type));

                PrintInheritanceOfType(writer, type);
                PrintGenericParametersOfType(writer, type);
                PrintPropertiesOfType(writer, type);
                PrintMethodsOfType(writer, type);
                writer.WriteEndElement();
            }

            private static void PrintStruct(XmlWriter writer, Type type)
            {
                writer.WriteStartElement("Struct");
                PrintEntityIdOfType(writer, type);

                writer.WriteAttributeString("name", GetNameWithoutGenericMark(type));

                PrintInheritanceOfType(writer, type);
                PrintGenericParametersOfType(writer, type);
                PrintFieldsOfType(writer, type);
                PrintPropertiesOfType(writer, type);
                PrintMethodsOfType(writer, type);
                PrintConstructorsOfType(writer, type);

                // Nested types
                foreach (Type nestedType in type.GetNestedTypes())
                {
                    PrintType(writer, nestedType);
                }

                writer.WriteEndElement();
            }

            private static void PrintClass(XmlWriter writer, Type type)
            {
                writer.WriteStartElement("Class");

                PrintEntityIdOfType(writer, type);

                writer.WriteAttributeString("name", GetNameWithoutGenericMark(type));

                PrintInheritanceOfType(writer, type);
                PrintGenericParametersOfType(writer, type);
                PrintFieldsOfType(writer, type);
                PrintPropertiesOfType(writer, type);
                PrintMethodsOfType(writer, type);
                PrintConstructorsOfType(writer, type);

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
                
                PrintEntityIdOfType(writer, type);

                foreach (var enumName in type.GetEnumNames())
                {
                    writer.WriteStartElement("EnumName");
                    if (Program.WriteEntityId)
                    {
                        writer.WriteAttributeString("entityId", $"{type.Namespace}.{type.Name}.{enumName}");
                    }

                    writer.WriteAttributeString("name", enumName);
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
            }

            private static void PrintDelegate(XmlWriter writer, Type type)
            {
                writer.WriteStartElement("Delegate");
                
                PrintEntityIdOfType(writer, type);
                writer.WriteAttributeString("name", GetNameWithoutGenericMark(type));

                var methodInfo = type.GetMethod("Invoke");
                writer.WriteAttributeString("return", GetCSharpFullName(methodInfo.ReturnType));

                // Generic parameters
                foreach (Type genericParam in type.GetGenericArguments())
                {
                    writer.WriteStartElement("GenericParameter");
                    if (Program.WriteEntityId)
                    {
                        writer.WriteAttributeString("entityId", $"{type.Namespace}.{type.Name}.#{genericParam.Name}");
                    }

                    writer.WriteAttributeString("name", genericParam.Name);
                    writer.WriteEndElement();
                }

                foreach (ParameterInfo parameter in methodInfo.GetParameters())
                {
                    writer.WriteStartElement("Parameter");
                    if (Program.WriteEntityId)
                    {
                        writer.WriteAttributeString("entityId", $"{type.Namespace}.{type.Name}.{parameter.Name}");
                    }

                    writer.WriteAttributeString("name", parameter.Name);
                    writer.WriteAttributeString("type", GetCSharpFullName(parameter.ParameterType));
                    writer.WriteAttributeString("ref", (parameter.ParameterType.IsByRef && !parameter.IsOut).ToString());
                    writer.WriteAttributeString("out", parameter.IsOut.ToString());
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
            }

            private static string GetNameWithoutGenericMark(Type type)
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
                    return $"#{type.Name}";
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