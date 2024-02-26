# Overview

**Nexus** is a versatile command-line tool designed for processing PDF and TIF files. It offers functionalities to convert, print, and combine these file types, catering to various document handling needs.

This project was meant for Windows to replace some old legacy software we were using to accomplish these tasks, most of these functions are native to Linux without the need for this tool.

**Note:** The printing of PDFs is handled by GhostScript. You will need to source the binaries from them and place them alongside your Nexus.exe.

GhostScript can be found at: [https://www.ghostscript.com/releases/gsdnld.html](https://www.ghostscript.com/releases/gsdnld.html)


# Build Instructions

1. **Clone the Repository**
git clone https://github.com/zzpixels/Nexus


2. **Change to project directory**
cd Nexus


3. **Restore NuGet Packages**
dotnet restore


4. **Build the project**
dotnet build


# Features

**Nexus** offers the following functionalities:

1. **Convert TIF to PDF:** Convert a TIF file to a PDF document.

2. **Print PDF:** Send a PDF file to a specified printer.

3. **Combine TIF Files:** Merge multiple TIF files into a single multi-page TIF file.

4. **Combine PDF Files:** Combine multiple single-page PDF files into a multi-page PDF document.


# Usage Instructions

1. **Convert TIF to PDF** - Converts a TIF file into a PDF document.

Command:
Nexus.exe -convert <TIF_FILE_PATH> <PDF_OUTPUT_PATH>

- `<TIF_FILE_PATH>`: Path of the TIF file to convert.
- `<PDF_OUTPUT_PATH>`: Path where the converted PDF will be saved.


2. **Print PDF** - Send a PDF file to a specified printer.

Command:
Nexus.exe -printpdf <PDF_FILE_PATH> <PRINTER_NAME>

- `<PDF_FILE_PATH>`: Path of the PDF file to print.
- `<PRINTER_NAME>`: Name of the printer.


3. **Combine TIF Files** - Merges multiple TIF files into a single multi-page TIF file.

Command:
Nexus.exe -combinetif <TIF1> <TIF2> <TIF3> <OUTPUT_TIF>

- `<TIF1> <TIF2> <TIF3> Continued...`: Paths of the TIF files to combine. (Can handle as many inputs as you feed it)
- `<OUTPUT_TIF>`: Path where the combined TIF file will be saved.

4. **Combine PDF Files** - Combines multiple PDF files into one multi-page PDF document.

Command:
Nexus.exe -combinepdf <PDF1> <PDF2> <PDF3> <OUTPUT_PDF>

- `<PDF1> <PDF2> <PDF3> Continued...`: Paths of the PDF files to combine. (Can handle as many inputs as you feed it)
- `<OUTPUT_PDF>`: Path where the combined PDF will be saved.
