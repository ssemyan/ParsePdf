# ParsePdf
Simple PDF to text parser to show how to use [iTextSharp](https://github.com/itext/itextsharp) with the [Azure Congnitive Computer Vision](https://azure.microsoft.com/en-us/services/cognitive-services/directory/vision/) service to extract text from a PDF whether it is a 'normal' or 'scanned' PDF. 

To use, first create a Cognitive Vision service in Azure or [here](https://azure.microsoft.com/en-us/try/cognitive-services). 

Then get the API key and API endpoint for the service (including the HTTPS) and put them in the app.config.

Once you have built it, to use it on the command line:

    ParsePdf [path to pdf file]
    
