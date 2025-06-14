using System.IO;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;


namespace FitCalc.Services
{
    public static class PdfHelper
    {
        public static byte[] CrearPdfDesdeTexto(string contenido)
        {
            using (var ms = new MemoryStream())
            {
                // Crea documento
                var document = new Document(PageSize.A4, 50, 50, 25, 25);
                PdfWriter.GetInstance(document, ms);
                document.Open();

                // Añade contenido
                var font = FontFactory.GetFont(FontFactory.HELVETICA, 12);
                document.Add(new Paragraph(contenido, font));

                document.Close();
                return ms.ToArray();
            }
        }
    }

}
