using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using Element = iTextSharp.text.Element;

namespace FitCalc.Services
{
    public static class PdfHelper
    {
        public static byte[] CrearPdfDesdeTexto(string contenido, string fecha)
        {
            using (var ms = new MemoryStream())
            {
                var document = new Document(PageSize.A4, 50, 50, 25, 25);
                PdfWriter.GetInstance(document, ms);
                document.Open();

                // Fuentes
                var fontHeader = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16, BaseColor.WHITE);
                var fontFecha = FontFactory.GetFont(FontFactory.HELVETICA, 12, BaseColor.GRAY);
                var fontContent = FontFactory.GetFont(FontFactory.HELVETICA, 12, BaseColor.BLACK);
                var fontHighlight = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.RED);

                // Cabecera con fondo azul
                PdfPTable headerTable = new PdfPTable(1) { WidthPercentage = 100 };
                PdfPCell headerCell = new PdfPCell(new Phrase("📝 Informe de alimentos del día", fontHeader))
                {
                    BackgroundColor = new BaseColor(70, 130, 180), // SteelBlue
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 10,
                    Border = Rectangle.NO_BORDER
                };
                headerTable.AddCell(headerCell);
                document.Add(headerTable);

                // Fecha centrada
                var fechaParagraph = new Paragraph(fecha, fontFecha)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 20
                };
                document.Add(fechaParagraph);

                // Crear tabla con una celda para todo el contenido
                PdfPTable contenidoTable = new PdfPTable(1)
                {
                    WidthPercentage = 100,
                    SpacingBefore = 10,
                    SpacingAfter = 20
                };

                PdfPCell contenidoCell = new PdfPCell()
                {
                    BackgroundColor = new BaseColor(240, 248, 255), // AliceBlue, suave y claro
                    Padding = 15,
                    BorderColor = new BaseColor(70, 130, 180),
                    BorderWidth = 2,
                    MinimumHeight = 400 // que ocupe más espacio en la página
                };

                // Para mantener saltos de línea, usar Phrase y agregar líneas
                var phrase = new Phrase();

                using (var reader = new System.IO.StringReader(contenido))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        // Detectar líneas con "Calorías totales" para resaltar
                        if (line.Contains("Calorías totales"))
                            phrase.Add(new Chunk(line + "\n", fontHighlight));
                        else
                            phrase.Add(new Chunk(line + "\n", fontContent));
                    }
                }

                contenidoCell.AddElement(phrase);
                contenidoTable.AddCell(contenidoCell);
                document.Add(contenidoTable);

                document.Close();
                return ms.ToArray();
            }
        }
    }
}
