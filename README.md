# DotNetLibraryExporter
Utility to extract public API of .NET library.

## Build
### Prerequisities
 - .NET Framework 4.5+ (version 4.8 recommended)
 
Repository contains Microsoft Visual Studio Solution File (.sln). It contains 3 projects file (exporter, tests and example dll). Both, solution and project files can be opened by one of the standard IDE for C#:
 - https://visualstudio.microsoft.com/vs/ VS2012+ (the community edition is sufficient)
 - https://www.monodevelop.com/
 - https://www.jetbrains.com/rider/

When you open it in your prefered IDE, you can build whole solution or individual projects through standard way.

## Usage
Once you have utility in binary form, you can run it from command line: 
```
DotNetFrameworkDllExporter.exe <path-to-extracting-dll-file> [-i] [-o <path-to-output-file>]
```
Parameter | Name | Parameters | Desription
--- | --- | --- | ---
`<input>` | Input File | | First required parameter used as source library which will be extracted.
**-i**    | interactive mode | | Program will be waiting for pressing any key after extracting library.
**-o**    | output | 1. Path to output file. | Specify file where to write output. It file exist it will be overridden. It none output file is specified standard output (stdout) will be used.
**-e**    | entity ID | | Output file will NOT contains attribute `entityId`, as a unique identifier.

## Features
 - Single Assembly extracting
 - Support namespaces and namespaces members: class, interface, enum
 - Support type members: fields, methods, properties, nested types

## Output Structure
Output is always xml file. By default its encoding is UTF-8. It can be different if the set output is not supporting this type of encoding, for example standard output.

Every output file has at the beginning xml version information. After that there is element `Assembly` which contains whole extracted API from one dll (Assembly). Whole file is hierarchical by namespaces and (non-nested) types. Every element has its unique entityId (this feature can be disableb by command line run parameter).

### Overview of elements:
Element   | attributes | C#/.NET origin | Meaning
---       | ---        | ---            | --- 
Namespace | name       | namespace      |
Interface | name       | interface      |
Class     | name       | class          |
Struct    | name       | struct         |
Delegate  | name       | delegate       |
Enum      | name       | enum           |
---       | ---        | ---            | --- 
Method    |            |                |
Parameter |            |                |                
Field     |            |                |
Property  |            |                | 


### Default DTD file ###
To every XML file created by this tool is prepended DTD file, this is how by default look like.

```
<!ELEMENT Assembly (Namespace|Interface|Class|Enum|Struct)*>
<!ELEMENT Namespace (Namespace|Interface|Class|Enum|Struct)*>
    <!ATTLIST Namespace entityId CDATA #REQUIRED>
    <!ATTLIST Namespace name CDATA #REQUIRED>

<!ELEMENT Interface (Method|Property)*>
    <!ATTLIST Interface entityId CDATA #REQUIRED>
    <!ATTLIST Interface name CDATA #REQUIRED>

<!ELEMENT Enum (#PCDATA)>
    <!ATTLIST Enum entityId CDATA #REQUIRED>
    <!ATTLIST Enum name CDATA #REQUIRED>

<!ELEMENT Class (Interface|Class|Enum|Struct|Field|Property|Method|Constructor|GenericParameter|BaseClass|InterfaceImplemented)*>
    <!ATTLIST Class entityId CDATA #REQUIRED>
    <!ATTLIST Class name CDATA #REQUIRED>
    <!ATTLIST Class BaseClass CDATA>
    <!ATTLIST Class InterfaceImplemented CDATA>
    <!ELEMENT GenericParameter (#PCDATA)>
        <!ATTLIST GenericParameter entityId CDATA #REQUIRED>
        <!ATTLIST GenericParameter name CDATA #REQUIRED>

<!ELEMENT Delegate (GenericParameter|Parameter)*>
    <!ATTLIST Delegate entityId CDATA #REQUIRED>
    <!ATTLIST Delegate name CDATA #REQUIRED>
    <!ATTLIST Delegate return CDATA #REQUIRED>

<!ELEMENT Struct (Interface|Class|Enum|Struct|Field|Property|Method|Constructor|GenericParameter|InterfaceImplemented)*>
    <!ATTLIST Struct entityId CDATA #REQUIRED>
    <!ATTLIST Struct name CDATA #REQUIRED>
    <!ATTLIST Struct InterfaceImplemented CDATA>

<!ELEMENT Field (#PCDATA)>
    <!ATTLIST Field entityId CDATA #REQUIRED>
    <!ATTLIST Field name CDATA #REQUIRED>
    <!ATTLIST Field static CDATA #REQUIRED>
    <!ATTLIST Field type CDATA #REQUIRED>

<!ELEMENT Property (#PCDATA)>
    <!ATTLIST Property entityId CDATA #REQUIRED>
    <!ATTLIST Property name CDATA #REQUIRED>
    <!ATTLIST Property type CDATA #REQUIRED>
    <!ATTLIST Property set (True|False) "False">
    <!ATTLIST Property get (True|False) #REQUIRED>

<!ELEMENT Method (GenericParameter|Parameter)*>
    <!ATTLIST Method entityId CDATA #REQUIRED>
    <!ATTLIST Method name CDATA #REQUIRED>
    <!ATTLIST Method static (True|False) #REQUIRED>
    <!ATTLIST Method return CDATA #REQUIRED>
    <!ELEMENT Parameter (#PCDATA)>
        <!ATTLIST Parameter entityId CDATA #REQUIRED>
        <!ATTLIST Parameter name CDATA #REQUIRED>
        <!ATTLIST Parameter type CDATA #REQUIRED>
        <!ATTLIST Parameter ref CDATA #REQUIRED>
        <!ATTLIST Parameter out CDATA #REQUIRED>

<!ELEMENT Constructor (Parameter)*>
```
