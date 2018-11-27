using System;
using System.IO;

namespace ParsePdf
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args == null || args.Length != 1)
			{
				Console.WriteLine("Usage: ParsePdf <file to parse>");
				return;
			}

			string fileName = args[0];
			if (!File.Exists(fileName))
			{
				Console.WriteLine("File {0} does not exist.", fileName);
				return;
			}

			var fileBytes = File.ReadAllBytes(fileName);
			string pdfText = PdfHelper.GetTextFromPdfBytes(fileBytes);
			Console.WriteLine("Text:");
			Console.WriteLine(pdfText);
		}
	}
}
