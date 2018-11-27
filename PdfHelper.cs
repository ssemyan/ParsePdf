using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace ParsePdf
{
    public static class PdfHelper
    {
        public static string GetTextFromPdfBytes(byte[] byteArray)
        {
            string pdfText = ParsePdfFromText(byteArray);
            if (string.IsNullOrEmpty(pdfText))
            {
                // Can't get it normally, extract from images
                var images = ExtractImages(byteArray);
                if (images.Count > 0)
                {
                    Debug.WriteLine("File has images. Trying to do OCR...");
                    foreach (var name in images.Keys)
                    {
                        //if there is a filetype save the file
                        if (name.LastIndexOf(".") + 1 != name.Length)
                        {
                            Debug.Write("Parsing " + name);

                            // Try to get the text
                            var converter = new System.Drawing.ImageConverter();
                            byte[] bytes = (byte[])converter.ConvertTo(images[name], typeof(byte[]));
                            string text = OcrHelper.DoOcr(bytes);
                            if (string.IsNullOrEmpty(text))
                            {
                                Debug.WriteLine("No text found in file " + name);
                            }
                            pdfText += text;                            
                        }
                        else
                        {
                            Debug.WriteLine("Unknown file for " + name);
                        }
                    }
                }
            }

            return pdfText;
        }

        private static string ParsePdfFromText(byte[] fileBytes)
        {
            string retStr = string.Empty;
            using (PdfReader reader = new PdfReader(fileBytes))
            {
                for (int i = 1; i <= reader.NumberOfPages; i++)
                {
                    var text = PdfTextExtractor.GetTextFromPage(reader, i, new SimpleTextExtractionStrategy());
                    retStr += text;
                }
            }
            return retStr;
        }

        private static Dictionary<string, System.Drawing.Image> ExtractImages(byte[] fileBytes)
        {
            var images = new Dictionary<string, System.Drawing.Image>();
            using (var reader = new PdfReader(fileBytes))
            {
                var parser = new PdfReaderContentParser(reader);

                for (var i = 1; i <= reader.NumberOfPages; i++)
                {
                    ImageRenderListener listener = new ImageRenderListener();
                    parser.ProcessContent(i, listener);
                    var index = 1;

                    if (listener.Images.Count > 0)
                    {
                        foreach (var pair in listener.Images)
                        {
                            images.Add(string.Format("Page_{0}_Image_{1}{2}", i.ToString("D4"), index.ToString("D4"), pair.Value), pair.Key);
                            index++;
                        }
                    }
                }
                return images;
            }
        }

        internal class ImageRenderListener : IRenderListener
        {
            Dictionary<System.Drawing.Image, string> images = new Dictionary<System.Drawing.Image, string>();

            public Dictionary<System.Drawing.Image, string> Images
            {
                get { return images; }
            }

            public void BeginTextBlock() { }

            public void EndTextBlock() { }

            public void RenderImage(ImageRenderInfo renderInfo)
            {
                PdfImageObject image = renderInfo.GetImage();
                PdfName filter = (PdfName)image.Get(PdfName.FILTER);

                if (filter != null)
                {
                    try
                    {
                        System.Drawing.Image drawingImage = image.GetDrawingImage();

                        string extension = ".";

                        if (Equals(filter, PdfName.DCTDECODE))
                        {
                            extension += PdfImageObject.ImageBytesType.JPG.FileExtension;
                        }
                        else if (Equals(filter, PdfName.JPXDECODE))
                        {
                            extension += PdfImageObject.ImageBytesType.JP2.FileExtension;
                        }
                        else if (Equals(filter, PdfName.FLATEDECODE))
                        {
                            extension += PdfImageObject.ImageBytesType.PNG.FileExtension;
                        }
                        else if (Equals(filter, PdfName.LZWDECODE))
                        {
                            extension += PdfImageObject.ImageBytesType.CCITT.FileExtension;
                        }
                        else if (Equals(filter, PdfName.CCITTFAXDECODE))
                        {
                            extension += PdfImageObject.ImageBytesType.CCITT.FileExtension;
                        }
                        else
                        {
                            Debug.WriteLine("Unknown type: " + filter);
                        }

                        Images.Add(drawingImage, extension);
                    }
                    catch (ArgumentException)
                    {
                        // unknow image type
                        Debug.WriteLine("Unknown image type.");
                    }
                }
            }

            public void RenderText(TextRenderInfo renderInfo) { }
        }

        public static byte[] ReadFully(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }
}
