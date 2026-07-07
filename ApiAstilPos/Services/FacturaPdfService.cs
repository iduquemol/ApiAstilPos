using ApiAstilPos.Models;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ApiAstilPos.Services
{
    public class FacturaPdfService
    {
        public FacturaPdfService()
        {
            // Configurar licencia Community (requerido)
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public byte[] GenerarPdfFactura(PrintVenta factura,int idMetodoDian)
        {
            var documento = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(8).FontFamily("Arial"));

                    page.Header().Element(header => ComposeHeader(header, factura, idMetodoDian));
                    // Combinar Content y Footer en uno solo
                    page.Content().Element(content => ComposeContentAndFooter(content, factura));
                });
            });

            return documento.GeneratePdf();
        }

        void ComposeHeader(IContainer container, PrintVenta factura, int idMetodoDian)
        {
            container.Column(column =>
            {
                // Fila superior: Logo, Info empresa y QR
                column.Item().Row(row =>
                {
                    // Logo - Desde bytes
                    row.RelativeItem(3).Height(100).Padding(5).AlignCenter().AlignMiddle()
                        .Image(CargarImagenLogo())
                        .FitArea();

                    // Información de la empresa
                    row.RelativeItem(4).PaddingHorizontal(10).Column(col =>
                    {
                        col.Item().AlignCenter().Text(factura.FacturadorNombre).Bold().FontSize(9);
                        col.Item().AlignCenter().Text($"{factura.FacturadorTipoId} : {factura.FacturadorNumeroIdentificacion}").FontSize(10);
                        col.Item().AlignCenter().Text(factura.FacturadorNombreComercial).Bold().FontSize(10);
                        col.Item().AlignCenter().Text(factura.FacturadorDireccion).FontSize(10);
                        col.Item().AlignCenter().Text($"{factura.FacturadorMunicipio}").FontSize(8);
                        col.Item().AlignCenter().Text($"{factura.notaDireccion}").FontSize(8);
                        col.Item().AlignCenter().Text($"Teléfono: {factura.FacturadorTelefono}").FontSize(8);
                        col.Item().AlignCenter().Text($"E-mail: {factura.FacturadorEmail}").FontSize(8);
                    });

                    // QR Code - GENERADO CON URL DE DIAN
                    row.RelativeItem(2).Height(100).Padding(5).AlignCenter().AlignMiddle()
                        .Image(GenerarQRCode(factura.CodigoQR ?? "N/A"))
                        .FitArea();
                });

                // Espacio
                column.Item().PaddingTop(10);

                // Resolución DIAN
                column.Item().Padding(3)
                    .Column(col =>
                    {
                        col.Item().Text(factura.NotaResolucion).FontSize(6);
                        col.Item().Text($"{factura.NotaTipoPersona} {factura.NotaRegimen} {factura.NotaTipoContribuyente} {factura.NotaAutorretendor}").Bold().FontSize(6);
                        col.Item().Text($"{factura.NotaFe1} {factura.NotaFe2}").Bold().FontSize(6);                        
                    });

                // Espacio
                column.Item().PaddingTop(5);

                // Representación gráfica
                if (idMetodoDian == 1)
                {
                    column.Item().AlignCenter().Background(Colors.Grey.Lighten4).Padding(3)
                    .Text($"Representación gráfica de la factura electrónica de venta")
                    .FontSize(8).Bold();
                }
                else if (idMetodoDian == 3)
                {
                    column.Item().AlignCenter().Background(Colors.Grey.Lighten4).Padding(3)
                    .Text($"Representación gráfica de la nota crédito electrónica")
                    .FontSize(8).Bold();
                }

                // CUFE
                column.Item().AlignCenter().Padding(3)
                    .Text(text =>
                    {
                        text.Span("CUFE: ").Bold().FontSize(8);
                        text.Span(factura.Cufe ?? "N/A").FontSize(8);
                    });

                // Espacio
                column.Item().PaddingTop(5);

                // Información del cliente y factura
                column.Item().Row(row =>
                {
                    // Datos del cliente
                    row.RelativeItem().Border(1).Padding(5).Column(col =>
                    {
                        col.Item().Text(text =>
                        {
                            text.Span("Cliente: ").Bold().FontSize(8);
                            text.Span(factura.ClienteRazonSocial).FontSize(8);
                        });
                        col.Item().Text(text =>
                        {
                            text.Span($"{factura.ClienteTipoId} : ").Bold().FontSize(8);
                            text.Span(factura.ClienteNumeroIdentificacion).FontSize(8);
                        });
                        col.Item().Text(text =>
                        {
                            text.Span("Dirección: ").Bold().FontSize(8);
                            text.Span(factura.ClienteDireccion ?? "N/A").FontSize(8);
                        });
                        col.Item().Text(text =>
                        {
                            text.Span("Teléfono: ").Bold().FontSize(8);
                            text.Span(factura.ClienteTelefono?.ToString() ?? "N/A").FontSize(8);
                        });
                        col.Item().Text(text =>
                        {
                            text.Span("Ciudad: ").Bold().FontSize(8);
                            text.Span(factura.ClienteMunicipio).FontSize(8);
                        });
                        col.Item().Text(text =>
                        {
                            text.Span("Correo electrónico: ").Bold().FontSize(8);
                            text.Span(factura.ClienteEmail).FontSize(8);
                        });
                        col.Item().Text(text =>
                        {
                            text.Span("Placa / Orden: ").Bold().FontSize(8);
                            text.Span(factura.OrdenReferencia).FontSize(8);
                        });
                    });

                    // Información de la factura
                    row.RelativeItem().Border(1).Padding(5).Column(col =>
                    {
                        if (idMetodoDian == 1)
                        {
                            col.Item().Text(text =>
                            {
                                text.Span("Factura Electrónica de Venta No. ").Bold().FontSize(8);
                                text.Span($"{factura.PrefijoVenta}{factura.NumeroVenta}").FontSize(8);
                            });
                        }
                        else if (idMetodoDian == 3)
                        {
                            col.Item().Text(text =>
                            {
                                text.Span("Nota Crédito de factura electrónica de venta No. ").Bold().FontSize(8);
                                text.Span($"{factura.PrefijoVenta}{factura.NumeroVenta}").FontSize(8);
                            });
                            col.Item().Text(text =>
                            {
                                text.Span("Documento de Origen:").Bold().FontSize(8);
                                text.Span($"{factura.DocumentoVenta}").FontSize(8);
                            });
                        }

                        col.Item().Text(text =>
                        {
                            text.Span("Tipo de operación: ").Bold().FontSize(8);
                            text.Span(factura.TipoOperacion).FontSize(8);
                        });
                        col.Item().Text(text =>
                        {
                            text.Span("Vendedor: ").Bold().FontSize(8);
                            text.Span(factura.NombreUsuario).FontSize(8);
                        });
                        col.Item().Text(text =>
                        {
                            text.Span("Fecha de Facturación: ").Bold().FontSize(8);
                            text.Span(FormatearFecha(factura.FechaVenta)).FontSize(8);
                        });
                        col.Item().Text(text =>
                        {
                            text.Span("Fecha de Validación: ").Bold().FontSize(8);
                            text.Span(FormatearFecha(factura.FechaHoraAutorizacion ?? "N/A")).FontSize(8);
                        });
                        col.Item().Text(text =>
                        {
                            text.Span("Fecha de Entrega: ").Bold().FontSize(8);
                            text.Span(FormatearFecha(factura.FechaEntrega)).FontSize(8);
                        });
                        col.Item().Text(text =>
                        {
                            text.Span("Fecha de Vencimiento: ").Bold().FontSize(8);
                            text.Span(factura.FechaVencimiento).FontSize(8);
                        });
                        col.Item().Text(text =>
                        {
                            text.Span("Medio de Pago: ").Bold().FontSize(8);
                            text.Span(factura.NombreMedioPago).FontSize(8);
                        });
                        col.Item().Text(text =>
                        {
                            text.Span("Forma de Pago: ").Bold().FontSize(8);
                            text.Span(factura.NombreFormaPago).FontSize(8);
                        });
                        col.Item().Row(innerRow =>
                        {
                            innerRow.RelativeItem().Text(text =>
                            {
                                text.Span("Plazo de Pago: ").Bold().FontSize(8);
                                text.Span(factura.PlazoPago).FontSize(8);
                            });
                            innerRow.RelativeItem().Text(text =>
                            {
                                text.Span("Moneda: ").Bold().FontSize(7);
                                text.Span(factura.Moneda).FontSize(7);
                            });
                        });
                    });
                });
            });
        }

        // Método auxiliar para cargar la imagen
        private byte[] CargarImagenLogo()
        {
            var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "images", "LogoServitrans.png");

            if (File.Exists(logoPath))
            {
                return File.ReadAllBytes(logoPath);
            }

            // Si no existe, retorna una imagen vacía o placeholder
            return new byte[0];
        }

        // MÉTODO ACTUALIZADO PARA GENERAR QR CODE
        private byte[] GenerarQRCode(string textoQR)
        {
            try
            {
                if (string.IsNullOrEmpty(textoQR) || textoQR == "N/A")
                {
                    textoQR = "https://catalogo-vpfe-hab.dian.gov.co/";
                }

                using (var qrGenerator = new QRCodeGenerator())
                {
                    // Crear QR Code con el link completo de la DIAN
                    var qrCodeData = qrGenerator.CreateQrCode(textoQR, QRCodeGenerator.ECCLevel.Q);
                    using (var qrCode = new PngByteQRCode(qrCodeData))
                    {
                        byte[] qrCodeImage = qrCode.GetGraphic(20); // 20 pixels por módulo
                        return qrCodeImage;
                    }
                }
            }
            catch (Exception ex)
            {
                // Si falla, retornar un QR con mensaje de error
                return GenerarQRPlaceholder($"Error: {ex.Message}");
            }
        }

        // Método auxiliar mejorado para generar un QR placeholder si falla
        private byte[] GenerarQRPlaceholder(string mensaje = "ERROR")
        {
            using (var qrGenerator = new QRCodeGenerator())
            {
                var qrCodeData = qrGenerator.CreateQrCode(mensaje, QRCodeGenerator.ECCLevel.Q);
                using (var qrCode = new PngByteQRCode(qrCodeData))
                {
                    return qrCode.GetGraphic(20);
                }
            }
        }

        void ComposeContentAndFooter(IContainer container, PrintVenta factura)
        {
            container.PaddingTop(10).Column(column =>
            {
                // Tabla de productos
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(4);  // Descripción
                        columns.RelativeColumn(1);  // Cantidad
                        columns.RelativeColumn(1);  // Valor Unidad
                        columns.RelativeColumn(1);  // Valor referencia
                        columns.RelativeColumn(1);  // % IVA
                        columns.RelativeColumn(1);  // % Desc
                        columns.RelativeColumn(1);  // Valor Total
                    });

                    // Header
                    table.Header(header =>
                    {
                        header.Cell().Border(1).Background(Colors.Grey.Lighten2).Padding(3)
                            .Text("Descripción").Bold().FontSize(8);
                        header.Cell().Border(1).Background(Colors.Grey.Lighten2).Padding(3)
                            .Text("Cantidad").Bold().FontSize(8);
                        header.Cell().Border(1).Background(Colors.Grey.Lighten2).Padding(3)
                            .Text("Valor Unidad").Bold().FontSize(8);
                        header.Cell().Border(1).Background(Colors.Grey.Lighten2).Padding(3)
                            .Text("Valor referencia por unidad").Bold().FontSize(7);
                        header.Cell().Border(1).Background(Colors.Grey.Lighten2).Padding(3)
                            .Text("% IVA").Bold().FontSize(8);
                        header.Cell().Border(1).Background(Colors.Grey.Lighten2).Padding(3)
                            .Text("% Desc.").Bold().FontSize(8);
                        header.Cell().Border(1).Background(Colors.Grey.Lighten2).Padding(3)
                            .Text("Valor Total").Bold().FontSize(8);
                    });

                    // Items
                    foreach (var item in factura.Items)
                    {
                        table.Cell().Border(1).Padding(3).Text(item.NombreProducto).FontSize(8);
                        table.Cell().Border(1).Padding(3).AlignCenter()
                            .Text($"{item.CantidadVenta:N2}\nUnidades").FontSize(8);
                        table.Cell().Border(1).Padding(3).AlignRight()
                            .Text($"$ COP {item.PrecioUnitarioVenta:N2}").FontSize(8);
                        table.Cell().Border(1).Padding(3).AlignRight()
                            .Text($"$ COP {item.ValorReferenciaUnidad:N2}").FontSize(8);
                        table.Cell().Border(1).Padding(3).AlignCenter()
                            .Text(item.PorcentajeIvaVenta.ToString()).FontSize(8);
                        table.Cell().Border(1).Padding(3).AlignCenter()
                            .Text($"{item.PorcentajeDescuentoVenta:N2}%").FontSize(8);
                        table.Cell().Border(1).Padding(3).AlignRight()
                            .Text($"$ COP {item.PrecioTotalVenta:N2}").FontSize(8);
                    }
                });

                // Espacio mínimo
                column.Item().PaddingTop(5);

                // Totales
                column.Item().Row(row =>
                {
                    // Valor en letras
                    row.RelativeItem().Border(1).Padding(5).Column(col =>
                    {
                        col.Item().Text(text =>
                        {
                            text.Span("Valor en letras: ").Bold().FontSize(8);
                            text.Span(factura.TotalVentaLetras).FontSize(8);
                        });
                    });

                    // Totales numéricos
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Border(1).Padding(3).Row(innerRow =>
                        {
                            innerRow.RelativeItem().Text("Total sin impuestos").FontSize(8);
                            innerRow.RelativeItem().AlignRight().Text($"$ COP {factura.TotalPrecio:N2}").FontSize(8);
                        });
                        col.Item().Border(1).Padding(3).Row(innerRow =>
                        {
                            innerRow.RelativeItem().Text("Descuentos Pie de Factura").FontSize(8);
                            innerRow.RelativeItem().AlignRight().Text($"$ COP {factura.TotalDescuento:N2}").FontSize(8);
                        });
                        col.Item().Border(1).Padding(3).Row(innerRow =>
                        {
                            innerRow.RelativeItem().Text("Fletes").FontSize(8);
                            innerRow.RelativeItem().AlignRight().Text("$ COP 0.00").FontSize(8);
                        });
                        col.Item().Border(1).Padding(3).Row(innerRow =>
                        {
                            innerRow.RelativeItem().Text($"IVA 19%").FontSize(8);
                            innerRow.RelativeItem().AlignRight().Text($"$ COP {factura.TotalIva:N2}").FontSize(8);
                        });
                        col.Item().Border(1).Padding(3).Row(innerRow =>
                        {
                            innerRow.RelativeItem().Text("RETENCIÓN FUENTE ").FontSize(8);
                            innerRow.RelativeItem().AlignRight().Text($"$ COP {factura.TotalReteRenta:N2}").FontSize(8);
                        });
                        col.Item().Border(1).Padding(3).Row(innerRow =>
                        {
                            innerRow.RelativeItem().Text("RETENCIÓN ICA ").FontSize(8);
                            innerRow.RelativeItem().AlignRight().Text($"$ COP {factura.TotalReteIca:N2}").FontSize(8);
                        });
                        col.Item().Border(1).Padding(3).Row(innerRow =>
                        {
                            innerRow.RelativeItem().Text("RETENCIÓN IVA ").FontSize(8);
                            innerRow.RelativeItem().AlignRight().Text($"$ COP {factura.TotalReteIva:N2}").FontSize(8);
                        });
                        col.Item().Border(1).Background(Colors.Grey.Lighten3).Padding(3).Row(innerRow =>
                        {
                            innerRow.RelativeItem().Text("Total a pagar").Bold().FontSize(9);
                            innerRow.RelativeItem().AlignRight().Text($"$ COP {factura.TotalVenta:N2}").Bold().FontSize(9);
                        });
                    });
                });

                // Espacio mínimo
                column.Item().PaddingTop(5);

                // Observaciones - AHORA PARTE DEL CONTENT
                column.Item().Border(1).Padding(5).Column(col =>
                {
                    col.Item().Text("Observaciones:").Bold().FontSize(8);
                    col.Item().Text(factura.observaciones1 ?? "").FontSize(8);

                    //col.Item().PaddingTop(5).Text("FAVOR CONSIGNAR EN LA CUENTA CORRIENTE No.03100005758 DE BANCOLOMBIA QUE SE ENCUENTRA BAJO NUESTRO NOMBRE").Bold().FontSize(7);
                });

                // Condiciones generales
                column.Item().PaddingTop(5).Border(1).Padding(5).Column(col =>
                {
                    col.Item().Text("Condiciones Generales:").Bold().FontSize(8);
                    col.Item().Text(factura.codicionesGenerales1 ?? "").FontSize(6);
                    col.Item().Text(factura.codicionesGenerales2 ?? "").FontSize(6);
                    col.Item().Text(factura.observaciones2 ?? "").FontSize(6);                    
                });

                // Firma y sello
                column.Item().PaddingTop(5).Row(row =>
                {
                    row.RelativeItem().Border(1).Height(60).Padding(5).Column(col =>
                    {
                        col.Item().AlignCenter().Text("Firma Autorizada").FontSize(8);
                    });

                    row.RelativeItem().Border(1).Height(60).Padding(5).Column(col =>
                    {
                        col.Item().AlignCenter().Text("RECIBÍ CONFORME:").Bold().FontSize(8);
                        col.Item().PaddingTop(20).AlignCenter().Text("Firma y Sello del Cliente").FontSize(8);
                    });

                    row.RelativeItem().Border(1).Height(60).Padding(5).Column(col =>
                    {
                        col.Item().AlignCenter().Text("FECHA DE RECIBIDA").Bold().FontSize(8);
                        col.Item().PaddingTop(10).AlignCenter().Text("Con la aceptación de esta factura se dan por aprobadas las condiciones de garantía mencionadas").FontSize(6);
                    });
                });

                // Nota del facturador
                column.Item().PaddingTop(5).Padding(5)
                    .Text(factura.notaFacturador ?? "").FontSize(6);


            });
        }
        private string FormatearFecha(string fecha)
        {
            try
            {
                if (string.IsNullOrEmpty(fecha))
                    return "N/A";

                // Intentar parsear la fecha
                if (DateTime.TryParse(fecha, out DateTime fechaParseada))
                {
                    return fechaParseada.ToString("dd/MM/yyyy HH:mm:ss");
                }

                // Si no se puede parsear, retornar el valor original
                return fecha;
            }
            catch
            {
                return fecha ?? "N/A";
            }
        }

    }
}