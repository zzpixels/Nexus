using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Drawing.Imaging;
using System.IO;
using PdfiumViewer;
using ImageMagick;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Image = iTextSharp.text.Image;
using System.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace Nexus
{
    class Program
    {
        static string LogFile = "nexus.log";

        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Log("Invalid arguments provided. Expected at least 2.");
                return;
            }

            string operation = args[0].ToLower();

            try
            {
                switch (operation)
                {
                    case "-convert":
                        if (args.Length != 3)
                        {
                            Log("Usage: -convert <tif-file-path> <pdf-output-path>");
                            return;
                        }
                        ConvertTifToPdf(args[1], args[2]);
                        break;

                    case "-printpdf":
                        if (args.Length != 3)
                        {
                            Log("Usage: -printpdf <pdf-file-path> <printer-name>");
                            return;
                        }
                        GhostPrintPDF(args[1], args[2]);
                        break;

                    case "-combinetif":
                        if (args.Length < 3)
                        {
                            Log("Usage: -combinetif <file1> <file2> ... <output-file-path>");
                            return;
                        }
                        CombineTifs(args[1..^1], args[^1]);
                        break;

                    case "-combinepdf":
                        if (args.Length < 3)
                        {
                            Log("Usage: -combinepdf <pdf1> <pdf2> ... <output-pdf>");
                            return;
                        }
                        CombinePDFs(args[1..^1], args[^1]);
                        break;

                    default:
                        Log("Unknown operation: " + operation);
                        break;
                }
            }
            catch (Exception ex)
            {
                Log("Unhandled exception in Main: " + ex.Message);
            }
        }

        private static void Log(string message)
        {
            try
            {
                File.AppendAllText(LogFile, $"[{DateTime.UtcNow:u}] {message}{Environment.NewLine}");
            }
            catch
            {

            }
        }

        private static void ConvertTifToPdf(string tifFilePath, string pdfFilePath)
        {
            try
            {
                using (var images = new MagickImageCollection())
                {
                    images.Read(tifFilePath);
                    using (var stream = new FileStream(pdfFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    using (var document = new Document())
                    {
                        PdfWriter.GetInstance(document, stream);
                        document.Open();

                        foreach (var image in images)
                        {
                            using (var ms = new MemoryStream())
                            {
                                image.Write(ms);
                                ms.Position = 0;

                                Image pdfImage = Image.GetInstance(ms.ToArray());
                                pdfImage.ScaleToFit(document.PageSize.Width, document.PageSize.Height);
                                pdfImage.Alignment = Element.ALIGN_MIDDLE;

                                document.Add(pdfImage);
                            }
                        }

                        document.Close();
                    }
                }

                Log($"Success: Converted {tifFilePath} to {pdfFilePath}");
            }
            catch (Exception ex)
            {
                Log($"Failed to convert {tifFilePath} to {pdfFilePath}. Error: {ex.Message}");
            }
        }

        private static void GhostPrintPDF(string pdfFilePath, string printerName)
        {
            try
            {
                string ghostscriptExe = "gswin64c.exe";
                string command = $"-sDEVICE=mswinpr2 -dBATCH -dNOPAUSE -dNOSAFER -sPAPERSIZE=letter -r300 -dPDFFitPage -sOutputFile=\"%printer%{printerName}\" \"{pdfFilePath}\"";

                var psi = new ProcessStartInfo
                {
                    FileName = ghostscriptExe,
                    Arguments = command,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = new Process { StartInfo = psi })
                {
                    process.Start();
                    process.WaitForExit();

                    Log($"Success: Sent {pdfFilePath} to printer {printerName}");
                }
            }
            catch (Exception ex)
            {
                Log($"Failed to print {pdfFilePath} on {printerName}. Error: {ex.Message}");
            }
        }

        private static void CombineTifs(string[] filePaths, string outputFilePath)
        {
            try
            {
                using (var images = new MagickImageCollection())
                {
                    foreach (var filePath in filePaths)
                    {
                        images.Add(new MagickImage(filePath));
                    }

                    images.Write(outputFilePath);
                }

                Log($"Success: Combined TIFFs into {outputFilePath}. Source files: {string.Join(", ", filePaths)}");
            }
            catch (Exception ex)
            {
                Log($"Failed to combine TIFFs to {outputFilePath}. Source files: {string.Join(", ", filePaths)}. Error: {ex.Message}");
            }
        }

        private static void CombinePDFs(string[] pdfFilePaths, string outputFilePath)
        {
            try
            {
                using (var outputPdf = new PdfSharp.Pdf.PdfDocument())
                {
                    foreach (var pdfFilePath in pdfFilePaths)
                    {
                        using (var inputPdf = PdfSharp.Pdf.IO.PdfReader.Open(pdfFilePath, PdfSharp.Pdf.IO.PdfDocumentOpenMode.Import))
                        {
                            foreach (PdfSharp.Pdf.PdfPage page in inputPdf.Pages)
                            {
                                outputPdf.AddPage(page);
                            }
                        }
                    }

                    outputPdf.Save(outputFilePath);
                }

                Log($"Success: Combined PDFs into {outputFilePath}. Source files: {string.Join(", ", pdfFilePaths)}");
            }
            catch (Exception ex)
            {
                Log($"Failed to combine PDFs to {outputFilePath}. Source files: {string.Join(", ", pdfFilePaths)}. Error: {ex.Message}");
            }
        }
    }
}
