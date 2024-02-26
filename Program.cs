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
using System.Drawing.Printing;


namespace Nexus
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                ShowUsage();
                return;
            }

            string operation = args[0].ToLower();

            switch (operation)
            {
                case "-convert":
                    if (args.Length != 3)
                    {
                        Console.WriteLine("Usage: -convert <tif-file-path> <pdf-output-path>");
                        return;
                    }
                    ConvertTifToPdf(args[1], args[2]);
                    break;
                case "-printpdf":
                    if (args.Length != 3)
                    {
                        Console.WriteLine("Usage: -ghostprintpdf <pdf-file-path> <printer-name>");
                        return;
                    }
                    GhostPrintPDF(args[1], args[2]);
                    break;
                case "-combinetif":
                    if (args.Length < 3)
                    {
                        Console.WriteLine("Usage: -combinetif <file1> <file2> ... <output-file-path>");
                        return;
                    }
                    CombineTifs(args[1..^1], args[^1]);
                    break;
                case "-combinepdf":
                    if (args.Length < 3)
                    {
                        Console.WriteLine("Usage: -combinepdf <pdf1> <pdf2> ... <output-pdf>");
                        return;
                    }
                    CombinePDFs(args[1..^1], args[^1]);
                    break;
                default:
                    ShowUsage();
                    break;
            }
        }

        private static void ShowUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  Convert TIF to PDF: -convert <tif-file-path> <pdf-output-path>");
            Console.WriteLine("  Print PDF: -printpdf <pdf-file-path> <printer-name>");
            Console.WriteLine("  Combine Tifs: -combinetif <file1> <file2> ... <output-file-path>");
            Console.WriteLine("  Combine PDFs: -combinepdf <file1> <file2> ... <output-file-path>");
        }

        private static void ConvertTifToPdf(string tifFilePath, string pdfFilePath)
        {
            using (var images = new MagickImageCollection())
            {
                images.Read(tifFilePath);
                using (var stream = new FileStream(pdfFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
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
            }
            Console.WriteLine($"Converted {tifFilePath} to {pdfFilePath}");
        }





        private static void PrintPDF2(string pdfFilePath, string printerName)
        {
            using (var pdfiumDocument = PdfiumViewer.PdfDocument.Load(pdfFilePath))
            {
                using (var printDocument = new System.Drawing.Printing.PrintDocument())
                {
                    printDocument.PrinterSettings.PrinterName = printerName;
                    printDocument.PrintController = new StandardPrintController();

                    int pageIndex = 0;

                    printDocument.PrintPage += (sender, e) =>
                    {
                        if (pageIndex < pdfiumDocument.PageCount)
                        {
                            // Calculate the scaling factor (slightly reduced)
                            float scaleX = (float)(e.PageBounds.Width - 30) / pdfiumDocument.PageSizes[pageIndex].Width;
                            float scaleY = (float)(e.PageBounds.Height - 30) / pdfiumDocument.PageSizes[pageIndex].Height;
                            float scaleFactor = Math.Min(scaleX, scaleY);

                            // Determine the size of the rendered image
                            int imageWidth = (int)(pdfiumDocument.PageSizes[pageIndex].Width * scaleFactor);
                            int imageHeight = (int)(pdfiumDocument.PageSizes[pageIndex].Height * scaleFactor);
                            using (var pageImage = pdfiumDocument.Render(pageIndex, imageWidth, imageHeight, 300, 300, PdfRenderFlags.ForPrinting)) // Increased DPI and ForPrinting flag
                            {
                                e.Graphics.DrawImage(pageImage, new System.Drawing.Rectangle((e.PageBounds.Width - imageWidth) / 2, (e.PageBounds.Height - imageHeight) / 2, imageWidth, imageHeight));
                            }
                            e.HasMorePages = ++pageIndex < pdfiumDocument.PageCount;
                        }
                    };

                    printDocument.Print();
                }
            }
            Console.WriteLine("Print job sent successfully.");
        }

        private static void PrintPDF(string pdfFilePath, string printerName)
        {
            using (var pdfiumDocument = PdfiumViewer.PdfDocument.Load(pdfFilePath))
            {
                using (var printDocument = new PrintDocument())
                {
                    printDocument.PrinterSettings.PrinterName = printerName;
                    printDocument.PrintController = new StandardPrintController();

                    int pageIndex = 0;

                    printDocument.PrintPage += (sender, e) =>
                    {
                        if (pageIndex < pdfiumDocument.PageCount)
                        {
                            // Reduce the margins to increase the scale of the printed image
                            // Example: Decreasing margins by 20 units on each side
                            var reducedMargin = 0;
                            var printableArea = e.MarginBounds;
                            printableArea.Inflate(-reducedMargin, -reducedMargin);

                            // Calculate the aspect ratio of the PDF page
                            double pdfAspectRatio = pdfiumDocument.PageSizes[pageIndex].Width / pdfiumDocument.PageSizes[pageIndex].Height;

                            // Calculate the scaling factor based on aspect ratio
                            double scale;
                            if (printableArea.Width / printableArea.Height > pdfAspectRatio)
                            {
                                // Fit to height
                                scale = printableArea.Height / pdfiumDocument.PageSizes[pageIndex].Height;
                            }
                            else
                            {
                                // Fit to width
                                scale = printableArea.Width / pdfiumDocument.PageSizes[pageIndex].Width;
                            }

                            // Calculate the size of the rendered image
                            int imageWidth = (int)(pdfiumDocument.PageSizes[pageIndex].Width * scale);
                            int imageHeight = (int)(pdfiumDocument.PageSizes[pageIndex].Height * scale);

                            // Render the page at the calculated size
                            using (var pageImage = pdfiumDocument.Render(pageIndex, imageWidth, imageHeight, 300, 300, PdfRenderFlags.ForPrinting))
                            {
                                // Center the image on the page
                                int x = printableArea.Left + (printableArea.Width - imageWidth) / 2;
                                int y = printableArea.Top + (printableArea.Height - imageHeight) / 2;

                                e.Graphics.DrawImage(pageImage, new System.Drawing.Rectangle(x, y, imageWidth, imageHeight));
                            }

                            e.HasMorePages = ++pageIndex < pdfiumDocument.PageCount;
                        }
                    };

                    printDocument.Print();
                }
            }
            Console.WriteLine("Print job sent successfully.");
        }


        private static void GhostPrintPDF(string pdfFilePath, string printerName)
        {
            // Path to Ghostscript executable
            string ghostscriptExe = "gswin64c.exe"; // Update with the correct Ghostscript executable name

            // Ghostscript command to print PDF
            string command = $"-sDEVICE=mswinpr2 -dBATCH -dNOPAUSE -dNOSAFER -sPAPERSIZE=letter -r300 -dPDFFitPage -sOutputFile=\"%printer%{printerName}\" \"{pdfFilePath}\"";

            // Create process start info
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = ghostscriptExe,
                Arguments = command,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            // Start the Ghostscript process
            using (Process process = new Process())
            {
                process.StartInfo = psi;
                process.Start();

                // Wait for the process to exit
                process.WaitForExit();

                Console.WriteLine("Print job sent successfully.");
            }
        }


        private static void CombineTifs(string[] filePaths, string outputFilePath)
        {
            using (var images = new MagickImageCollection())
            {
                foreach (var filePath in filePaths)
                {
                    images.Add(new MagickImage(filePath));
                }

                images.Write(outputFilePath);
            }
            Console.WriteLine($"Combined Tifs into {outputFilePath}");
        }

        private static void CombinePDFs(string[] pdfFilePaths, string outputFilePath)
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
            Console.WriteLine($"Combined PDFs into {outputFilePath}");
        }




    }
}

