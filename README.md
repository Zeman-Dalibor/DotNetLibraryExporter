# DotNetLibraryExporter
Utility to extract public API of .NET library.

## Build
### Prerequisities
 - .NET Framework 4.5+ (version 4.8 recommended)
 
Repository contains Microsoft Visual Studio Solution File (.sln). It contains 3 projects file (exporter, tests and example dll). Both, solution and project files can be opened by one of the standard IDE for C#:
 - https://visualstudio.microsoft.com/vs/ VS2012+ (the community edition is sufficient)
 - https://www.monodevelop.com/
 - https://www.jetbrains.com/rider/

When you open it in you prefered IDE, you can Build whole solution or individual projects through standard way.

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

## Features
 - Single Assembly extracting
 - Support namespaces and namespaces members: class, interface, enum
 - Support type members: fields, methods, properties, nested types

## Output Structure
Output is always xml file. By default its encoding is UTF-8. It can be different if the set output is not supporting this type of encoding, for example standard output.

Every outputing file has at the beginning xml version information. After that there is element `Assembly` which contains whole extracted API from one dll (Assembly). Whole file is hierarchical by namespaces and (non-nested) types.

### Overview of elements:
Element   | attributes | C#/.NET origin | Meaning
---       | ---        | ---            | --- 
Namespace | name       | namespace      |
Interface | name       | interface      |
Class     | name       | class          |
Enum      | name       | enum           |
---       | ---        | ---            | --- 
Method    |            |                |
Parameter |            |                |                
Field     |            |                |
Property  |            |                | 

