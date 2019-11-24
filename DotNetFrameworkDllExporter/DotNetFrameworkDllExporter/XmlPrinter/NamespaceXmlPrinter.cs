namespace DotNetFrameworkDllExporter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Xml;
    using DotNetFrameworkDllExporter.XmlPrinter;

    internal class NamespaceXmlPrinter
    {
        private readonly XmlWriter writer;
        private readonly EntityIdPrinter entityIdPrinter;

        public NamespaceXmlPrinter(XmlWriter writer, EntityIdPrinter entityIdPrinter)
        {
            this.writer = writer;
            this.entityIdPrinter = entityIdPrinter;
        }

        public void PrintNamespace(Namespace @namespace)
        {
            this.writer.WriteStartElement("Model");
            this.writer.WriteAttributeString("entityId", $"model-{@namespace.GetFullNamespace()}");
            this.writer.WriteAttributeString("name", @namespace.GetFullNamespace());

            foreach (Type type in @namespace.Types)
            {
                if (this.IsExcludedType(type))
                {
                    continue;
                }

                this.entityIdPrinter.AddEntityId($"{type.Namespace.Replace('.', '-')}-{type.Name}", EntityIdPrinter.ElementType.Module);
                string[] tokens = type.Namespace.Split('.');

                foreach (string namespacePart in tokens)
                {
                    this.writer.WriteStartElement("Namespace");
                    this.entityIdPrinter.PrintStartElementEntityId(namespacePart, EntityIdPrinter.ElementType.Concept);
                    this.writer.WriteAttributeString("name", namespacePart);
                }

                this.PrintType(type);

                for (int i = 0; i < tokens.Length; i++)
                {
                    this.entityIdPrinter.LeaveElement();
                    this.writer.WriteEndElement();
                }

                this.entityIdPrinter.RemoveEntityId();
            }

            foreach (KeyValuePair<string, Namespace> nameSpace in @namespace.InnerNamespaces)
            {
                this.PrintNamespace(nameSpace.Value);
            }
            
            this.writer.WriteEndElement();
        }

        private void PrintType(Type type)
        {
            if (type.IsClass)
            {
                if (type.IsSubclassOf(typeof(MulticastDelegate)))
                {
                    this.PrintDelegate(type);
                    return;
                }
                else
                {
                    this.PrintClass(type);
                    return;
                }
            }
            else if (type.IsInterface)
            {
                this.PrintInterface(type);
                return;
            }
            else if (type.IsValueType)
            {
                // Enum is also ValueType It must be checked first
                if (type.IsEnum)
                {
                    this.PrintEnum(type);
                    return;
                }
                else
                {
                    this.PrintStruct(type);
                    return;
                }
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        private void PrintEntityIdOfType(Type type)
        {
            this.entityIdPrinter.PrintStartElementEntityId(type.Name, EntityIdPrinter.ElementType.Concept);
        }

        private void PrintInheritanceOfType(Type type)
        {
            // Inheritance
            if (type.BaseType != null)
            {
                this.writer.WriteAttributeString("BaseClass", this.GetCSharpFullName(type.BaseType));
            }

            // Interface implement
            if (type.GetInterfaces().Length > 0)
            {
                this.writer.WriteAttributeString("InterfaceImplemented", string.Join(";", type.GetInterfaces().Select(interfaceType => this.GetCSharpFullName(interfaceType))));
            }
        }

        private void PrintConstructorsOfType(Type type)
        {
            foreach (ConstructorInfo constructorInfo in type.GetConstructors())
            {
                // Skip constructors with parameter type pointer
                if (constructorInfo.GetParameters().FirstOrDefault(par => this.IsExcludedType(par.ParameterType)) != null)
                {
                    continue;
                }

                this.writer.WriteStartElement("Constructor");
                string methodParamsString = string.Join(",", constructorInfo.GetParameters().Select(mt => this.GetCSharpFullName(mt.ParameterType)));

                this.entityIdPrinter.PrintStartElementEntityId($"{type.Name}({methodParamsString})", EntityIdPrinter.ElementType.Concept);

                foreach (ParameterInfo parameter in constructorInfo.GetParameters())
                {
                    this.writer.WriteStartElement("Parameter");
                    this.entityIdPrinter.PrintStartElementEntityId(parameter.Name, EntityIdPrinter.ElementType.Concept);

                    this.writer.WriteAttributeString("name", parameter.Name);
                    this.writer.WriteAttributeString("type", this.GetCSharpFullName(parameter.ParameterType));
                    this.writer.WriteAttributeString("ref", (parameter.ParameterType.IsByRef && !parameter.IsOut).ToString());
                    this.writer.WriteAttributeString("out", parameter.IsOut.ToString());

                    this.entityIdPrinter.LeaveElement();
                    this.writer.WriteEndElement();
                }

                this.entityIdPrinter.LeaveElement();
                this.writer.WriteEndElement();
            }
        }

        private void PrintMethodsOfType(Type type)
        {
            IEnumerable<string> propertiesNames = type.GetProperties().Select(property => property.Name);

            // Methods
            foreach (MethodInfo methodInfo in type.GetMethods())
            {
                // Skip methods with return type pointer
                if (this.IsExcludedType(methodInfo.ReturnType))
                {
                    continue;
                }

                // Skip methods with parameter type pointer
                if (methodInfo.GetParameters().FirstOrDefault(par => this.IsExcludedType(par.ParameterType)) != null)
                {
                    continue;
                }

                // Skip auto-generated methods from properties
                if (methodInfo.Name.StartsWith("set_") || methodInfo.Name.StartsWith("get_"))
                {
                    string potentialPropertyName = methodInfo.Name.Substring(4);
                    if (propertiesNames.Contains(potentialPropertyName))
                    {
                        continue;
                    }
                }

                if (type.IsInterface)
                {
                    this.writer.WriteStartElement("InterfaceMethod");
                }
                else
                {
                    this.writer.WriteStartElement("Method");
                }

                string genericParamsString = string.Join(",", methodInfo.GetGenericArguments().Select(mt => this.GetCSharpFullName(mt)));
                string methodParamsString = string.Join(",", methodInfo.GetParameters().Select(mt => this.GetCSharpFullName(mt.ParameterType)));
                this.entityIdPrinter.PrintStartElementEntityId($"{methodInfo.Name}{{{genericParamsString}}}({methodParamsString})", EntityIdPrinter.ElementType.Concept);

                this.writer.WriteAttributeString("name", methodInfo.Name);
                this.writer.WriteAttributeString("static", methodInfo.IsStatic.ToString());
                this.writer.WriteAttributeString("return", this.GetCSharpFullName(methodInfo.ReturnType));

                // Generic parameters
                foreach (Type genericParam in methodInfo.GetGenericArguments())
                {
                    this.writer.WriteStartElement("GenericParameter");
                    this.entityIdPrinter.PrintStartElementEntityId($"#{genericParam.Name}", EntityIdPrinter.ElementType.Concept);

                    this.writer.WriteAttributeString("name", genericParam.Name);
                    this.entityIdPrinter.LeaveElement();
                    this.writer.WriteEndElement();
                }

                foreach (ParameterInfo parameter in methodInfo.GetParameters())
                {
                    this.writer.WriteStartElement("Parameter");
                    this.entityIdPrinter.PrintStartElementEntityId(parameter.Name, EntityIdPrinter.ElementType.Concept);

                    this.writer.WriteAttributeString("name", parameter.Name);
                    this.writer.WriteAttributeString("type", this.GetCSharpFullName(parameter.ParameterType));
                    this.writer.WriteAttributeString("ref", (parameter.ParameterType.IsByRef && !parameter.IsOut).ToString());
                    this.writer.WriteAttributeString("out", parameter.IsOut.ToString());
                    this.entityIdPrinter.LeaveElement();
                    this.writer.WriteEndElement();
                }

                this.entityIdPrinter.LeaveElement();
                this.writer.WriteEndElement();
            }
        }

        private void PrintPropertiesOfType(Type type)
        {
            // Properties
            foreach (PropertyInfo propertyInfo in type.GetProperties())
            {
                // Skip properties with return type pointer
                if (this.IsExcludedType(propertyInfo.PropertyType))
                {
                    continue;
                }

                if (type.IsInterface)
                {
                    this.writer.WriteStartElement("InterfaceProperty");
                }
                else
                {
                    this.writer.WriteStartElement("Property");
                }

                this.entityIdPrinter.PrintStartElementEntityId(propertyInfo.Name, EntityIdPrinter.ElementType.Concept);

                this.writer.WriteAttributeString("name", propertyInfo.Name);
                this.writer.WriteAttributeString("type", this.GetCSharpFullName(propertyInfo.PropertyType));
                this.writer.WriteAttributeString("get", (propertyInfo.GetMethod != null).ToString());
                this.writer.WriteAttributeString("set", (propertyInfo.SetMethod != null).ToString());
                this.entityIdPrinter.LeaveElement();
                this.writer.WriteEndElement();
            }
        }

        private void PrintFieldsOfType(Type type)
        {
            // Fields
            foreach (FieldInfo fieldInfo in type.GetFields())
            {
                if (this.IsExcludedType(fieldInfo.FieldType))
                {
                    continue;
                }

                this.writer.WriteStartElement("Field");
                this.entityIdPrinter.PrintStartElementEntityId(fieldInfo.Name, EntityIdPrinter.ElementType.Concept);

                this.writer.WriteAttributeString("name", fieldInfo.Name);
                this.writer.WriteAttributeString("static", fieldInfo.IsStatic.ToString());
                this.writer.WriteAttributeString("type", this.GetCSharpFullName(fieldInfo.FieldType));
                this.entityIdPrinter.LeaveElement();
                this.writer.WriteEndElement();
            }
        }

        private void PrintGenericParametersOfType(Type type)
        {
            // Generic parameters
            foreach (Type genericParam in type.GetGenericArguments())
            {
                this.writer.WriteStartElement("GenericParameter");
                this.entityIdPrinter.PrintStartElementEntityId($"#{genericParam.Name}", EntityIdPrinter.ElementType.Concept);

                this.writer.WriteAttributeString("name", genericParam.Name);
                this.entityIdPrinter.LeaveElement();
                this.writer.WriteEndElement();
            }
        }

        private void PrintInterface(Type type)
        {
            this.writer.WriteStartElement("Interface");
            this.PrintEntityIdOfType(type);

            this.writer.WriteAttributeString("name", this.GetNameWithoutGenericMark(type));

            this.PrintInheritanceOfType(type);
            this.PrintGenericParametersOfType(type);
            this.PrintPropertiesOfType(type);
            this.PrintMethodsOfType(type);
            this.entityIdPrinter.LeaveElement();
            this.writer.WriteEndElement();
        }

        private void PrintStruct(Type type)
        {
            this.writer.WriteStartElement("Struct");
            this.PrintEntityIdOfType(type);

            this.writer.WriteAttributeString("name", this.GetNameWithoutGenericMark(type));

            this.PrintInheritanceOfType(type);
            this.PrintGenericParametersOfType(type);
            this.PrintFieldsOfType(type);
            this.PrintPropertiesOfType(type);
            this.PrintMethodsOfType(type);
            this.PrintConstructorsOfType(type);

            // Nested types
            foreach (Type nestedType in type.GetNestedTypes())
            {
                if (this.IsExcludedType(nestedType))
                {
                    continue;
                }

                this.PrintType(nestedType);
            }

            this.entityIdPrinter.LeaveElement();
            this.writer.WriteEndElement();
        }

        private void PrintClass(Type type)
        {
            this.writer.WriteStartElement("Class");

            this.PrintEntityIdOfType(type);

            this.writer.WriteAttributeString("name", this.GetNameWithoutGenericMark(type));

            this.PrintInheritanceOfType(type);
            this.PrintGenericParametersOfType(type);
            this.PrintFieldsOfType(type);
            this.PrintPropertiesOfType(type);
            this.PrintMethodsOfType(type);
            this.PrintConstructorsOfType(type);

            // Nested types
            foreach (Type nestedType in type.GetNestedTypes())
            {
                if (this.IsExcludedType(nestedType))
                {
                    continue;
                }

                this.PrintType(nestedType);
            }

            this.entityIdPrinter.LeaveElement();
            this.writer.WriteEndElement();
        }

        private void PrintEnum(Type type)
        {
            this.writer.WriteStartElement("Enum");
            this.writer.WriteAttributeString("name", this.GetNameWithoutGenericMark(type));

            this.PrintEntityIdOfType(type);

            foreach (string enumMember in type.GetEnumNames())
            {
                this.writer.WriteStartElement("EnumMember");
                this.entityIdPrinter.PrintStartElementEntityId(enumMember, EntityIdPrinter.ElementType.Concept);

                this.writer.WriteAttributeString("name", enumMember);
                this.entityIdPrinter.LeaveElement();
                this.writer.WriteEndElement();
            }

            this.entityIdPrinter.LeaveElement();
            this.writer.WriteEndElement();
        }

        private void PrintDelegate(Type type)
        {
            this.writer.WriteStartElement("Delegate");

            this.PrintEntityIdOfType(type);
            this.writer.WriteAttributeString("name", this.GetNameWithoutGenericMark(type));

            MethodInfo methodInfo = type.GetMethod("Invoke");
            this.writer.WriteAttributeString("return", this.GetCSharpFullName(methodInfo.ReturnType));

            this.PrintGenericParametersOfType(type);

            foreach (ParameterInfo parameter in methodInfo.GetParameters())
            {
                this.writer.WriteStartElement("Parameter");
                this.entityIdPrinter.PrintStartElementEntityId(parameter.Name, EntityIdPrinter.ElementType.Concept);

                this.writer.WriteAttributeString("name", parameter.Name);
                this.writer.WriteAttributeString("type", this.GetCSharpFullName(parameter.ParameterType));
                this.writer.WriteAttributeString("ref", (parameter.ParameterType.IsByRef && !parameter.IsOut).ToString());
                this.writer.WriteAttributeString("out", parameter.IsOut.ToString());
                this.entityIdPrinter.LeaveElement();
                this.writer.WriteEndElement();
            }

            this.entityIdPrinter.LeaveElement();
            this.writer.WriteEndElement();
        }

        private string GetNameWithoutGenericMark(Type type)
        {
            return type.Name.Split(new[] { '`' }, 2)[0];
        }

        private string GetCSharpFullName(Type type)
        {
            string prefix = $"{type.Namespace.Replace('.', '-')}-{type.Name}";

            if (type.IsByRef)
            {
                return $"{this.GetCSharpFullName(type.GetElementType())}";
            }
            else if (type.IsPointer)
            {
                return $"{this.GetCSharpFullName(type.GetElementType())}*";
            }
            else if (type.IsArray)
            {
                return $"{this.GetCSharpFullName(type.GetElementType())}[]";
            }
            else if (type.IsConstructedGenericType || type.IsGenericType)
            {
                return $"{this.GetGenericTypeFormat(type)}";
            }
            else if (type.IsGenericParameter)
            {
                return $"#{type.Name}";
            }
            else if (type.IsNested)
            {
                return $"{this.GetCSharpFullName(type.DeclaringType)}.{type.Name}";
            }
            else
            {
                return $"{prefix}:{type.FullName}";
            }
        }

        private string GetGenericTypeFormat(Type type)
        {
            Type[] genericTypes = type.GetGenericArguments();
            return this.GetReformatGeneric(type, genericTypes, genericTypes.Length);
        }

        private string GetReformatGeneric(Type type, Type[] genericTypes, int length)
        {
            this.ParseNameAndGenericParameterCount(type, out string name, out int genericParamsCount);

            var genericParams = genericTypes.Take(length).Skip(length - genericParamsCount).ToList();
            string genericTypesString = string.Join(", ", genericParams.Select(t => this.GetCSharpFullName(t)));

            if (type.IsNested)
            {
                string genericAppendix = string.IsNullOrWhiteSpace(genericTypesString) ? string.Empty : "{" + genericTypesString + "}";

                return this.GetReformatGeneric(type.DeclaringType, genericTypes, length - genericParamsCount) + "." + name + genericAppendix;
            }
            else
            {
                if (genericParamsCount != length)
                {
                    throw new InvalidOperationException();
                }

                string genericAppendix = string.IsNullOrWhiteSpace(genericTypesString) ? string.Empty : "{" + genericTypesString + "}";

                string prefix = $"{type.Namespace.Replace('.', '-')}-{type.Name}";

                return $"{prefix}:{type.Namespace}.{name}{genericAppendix}";
            }
        }

        private void ParseNameAndGenericParameterCount(Type type, out string name, out int numberOfGenericParams)
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

        private bool IsExcludedType(Type returnType)
        {
            if (this.GetCSharpFullName(returnType).Contains("*"))
            {
                return true;
            }
            else if (Program.ExcludedTypes.Contains(returnType))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}