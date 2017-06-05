# SLExtractNet
Extractor for Microsoft StarLancer .hog files

## Usage

   slextractnet.exe <hogfile>

Creates a new directory named after the .hog file in the current working directory and extracts all the contents into it.

## Building
Requires C# 6.0 or later to build because of the use of [Interpolated Strings](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/interpolated-strings), but can be modified to use [String.Format](https://msdn.microsoft.com/en-us/library/system.string.format(v=vs.110).aspx) with minimal effort.

