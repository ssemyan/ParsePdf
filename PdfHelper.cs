using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System.Collections.Generic;
using System.Diagnostics;

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
					foreach (var image in images)
					{
						// Try to get the text
						Debug.WriteLine("Doing OCR on image...");
						var converter = new System.Drawing.ImageConverter();
						byte[] bytes = (byte[])converter.ConvertTo(image, typeof(byte[]));
						string text = OcrHelper.DoOcr(bytes);
						if (string.IsNullOrEmpty(text))
						{
							Debug.WriteLine("No text found in file.");
						}
						pdfText += text;
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

        private static List<System.Drawing.Image> ExtractImages(byte[] fileBytes)
        {
            var images = new List<System.Drawing.Image>();
            using (var reader = new PdfReader(fileBytes))
            {
                var parser = new PdfReaderContentParser(reader);

                for (var i = 1; i <= reader.NumberOfPages; i++)
                {
                    ImageRenderListener listener = new ImageRenderListener();
                    parser.ProcessContent(i, listener);
                    images.AddRange(listener.Images);
                }
                return images;
            }
        }

        internal class ImageRenderListener : IRenderListener
        {
			public List<System.Drawing.Image> Images { get; } = new List<System.Drawing.Image>();

			public void BeginTextBlock() { }

            public void EndTextBlock() { }

			public void RenderImage(ImageRenderInfo renderInfo)
			{
				PdfImageObject image = renderInfo.GetImage();
				System.Drawing.Image drawingImage = image.GetDrawingImage();
				Images.Add(drawingImage);
			}

            public void RenderText(TextRenderInfo renderInfo) { }
        }
    }
}
