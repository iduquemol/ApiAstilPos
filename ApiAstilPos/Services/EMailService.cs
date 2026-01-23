using ApiAstilPos.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Data.SqlTypes;
using System.Threading.Tasks;

namespace ApiAstilPos.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendFacturaEmailAsync(FacturaEmailDto facturaData, int idMetodoDian)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("IT QUALIS S.A.S", _configuration["Email:Username"]));
                message.To.Add(new MailboxAddress(facturaData.NombreCliente, facturaData.Email));
                message.Subject = facturaData.SubjectEmail ?? $"Factura Electrónica N° {facturaData.NumeroDocumento}";
                var bodyBuilder = new BodyBuilder();

                if (idMetodoDian == 1)
                {
                    bodyBuilder = new BodyBuilder
                    {
                        HtmlBody = GenerarHtmlFactura(facturaData)
                    };
                }
                else if (idMetodoDian == 3)
                {
                    bodyBuilder = new BodyBuilder
                    {
                        HtmlBody = GenerarHtmlNota(facturaData)
                    };
                }


                // Crear archivo ZIP
                using (var memoryStream = new MemoryStream())
                {
                    using (var archive = new System.IO.Compression.ZipArchive(memoryStream, System.IO.Compression.ZipArchiveMode.Create, true))
                    {
                        // Agregar PDF al ZIP
                        var pdfEntry = archive.CreateEntry(facturaData.PdfFileName);
                        using (var entryStream = pdfEntry.Open())
                        {
                            await entryStream.WriteAsync(facturaData.PdfAttachment, 0, facturaData.PdfAttachment.Length);
                        }

                        // Agregar XML al ZIP si existe
                        if (facturaData.XmlAttachment != null && facturaData.XmlAttachment.Length > 0)
                        {
                            var xmlEntry = archive.CreateEntry(facturaData.XmlFileName);
                            using (var entryStream = xmlEntry.Open())
                            {
                                await entryStream.WriteAsync(facturaData.XmlAttachment, 0, facturaData.XmlAttachment.Length);
                            }
                        }
                    }

                    memoryStream.Position = 0;
                    var zipBytes = memoryStream.ToArray();

                    // Convertir el ZIP a Base64
                    string zipBase64 = Convert.ToBase64String(zipBytes);
                    string fileName = String.Empty;

                    if (idMetodoDian == 1)
                    {
                        fileName = $"Factura_{facturaData.NumeroDocumento}.zip";
                    }
                    else if (idMetodoDian == 3)
                    {
                        fileName = $"NotaCredito_{facturaData.NumeroDocumento}.zip";
                    }

                    bodyBuilder.Attachments.Add(
                        fileName,
                        Convert.FromBase64String(zipBase64),
                        ContentType.Parse("application/zip")
                    );
                }

                // Adjuntar PDF si existe
                //if (facturaData.PdfAttachment != null && facturaData.PdfAttachment.Length > 0)
                //{
                //    bodyBuilder.Attachments.Add(
                //        facturaData.PdfFileName ?? "Factura.pdf",
                //        facturaData.PdfAttachment,
                //        ContentType.Parse("application/pdf")
                //    );
                //}

                // Adjuntar XML si existe
                //if (facturaData.XmlAttachment != null && facturaData.XmlAttachment.Length > 0)
                //{
                //    bodyBuilder.Attachments.Add(
                //        facturaData.XmlFileName ?? "Factura.xml",
                //        facturaData.XmlAttachment,
                //        ContentType.Parse("application/xml")
                //    );
                //}

                message.Body = bodyBuilder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(
                        _configuration["Email:SmtpServer"] ?? "smtp-mail.outlook.com",
                        int.Parse(_configuration["Email:SmtpPort"] ?? "587"),
                        SecureSocketOptions.StartTls
                    );

                    await client.AuthenticateAsync(
                        _configuration["Email:Username"],
                        _configuration["Email:Password"]
                    );

                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al enviar email: {ex.Message}", ex);
            }
        }

        private string GenerarHtmlFactura(FacturaEmailDto facturaData)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset=""UTF-8"">
                    <style>
                        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; }}
                        .container {{ max-width: 600px; margin: 0 auto; }}
                        .header {{ text-align: left; margin-bottom: 20px; }}
                        .header h1 {{ margin: 0 0 5px 0; font-size: 16px; font-weight: 400; }}
                        .header h2 {{ margin: 0; font-size: 16px; font-weight: bold; }}
                        hr {{ border: none; border-top: 1px solid #ccc; margin: 20px 0; }}
                        p {{ font-size: 14px; line-height: 1.6; margin: 0 0 15px 0; text-align: justify; }}
                        .company-name {{ font-size: 14px; font-weight: bold; margin-top: 30px; }}
                        .legal {{ color: #666; font-size: 11px; line-height: 1.4; margin-top: 20px; }}
                    </style>
                </head>
                <body>
                    <div class=""container"">
                        <div class=""header"">
                            <h1>Su Factura</h1>
                            <h2>{facturaData.NumeroDocumento}</h2>
                        </div>
                        
                        <hr>
                        
                        <p>Adjunto realizamos envío de la siguiente documentación:</p>
                        
                        <p>
                            Representación Gráfica de la Factura Electrónica N° {facturaData.NumeroDocumento}.
                        </p>
                        
                        <p>
                            Factura Electrónica N° {facturaData.NumeroDocumento} por un importe de ${facturaData.Total:N2} de {facturaData.FacturadorNombre}. 
                            Consulte los detalles de plazo y modo de pago en el documento.
                        </p>
                        
                        <p>En caso de requerir cualquier aclaración, por favor no dude en contactarnos.</p>
                        
                        <p>Saludos cordiales,</p>
                        
                        <hr>
                        
                        <p class=""company-name"">{facturaData.FacturadorNombre}</p>
                        
                        <hr>
                        
                        <p class=""legal"">
                            <strong>AVISO:</strong> Este correo electrónico y sus documentos adjuntos son confidenciales y para uso exclusivo 
                            de la persona o entidad a la cual está dirigido. Si no es usted el destinatario, cualquier retención, difusión, 
                            distribución o copia de este mensaje está prohibida.
                        </p>
                    </div>
                </body>
                </html>
            ";
        }

        private string GenerarHtmlNota(FacturaEmailDto facturaData)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset=""UTF-8"">
                    <style>
                        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; }}
                        .container {{ max-width: 600px; margin: 0 auto; }}
                        .header {{ text-align: left; margin-bottom: 20px; }}
                        .header h1 {{ margin: 0 0 5px 0; font-size: 16px; font-weight: 400; }}
                        .header h2 {{ margin: 0; font-size: 16px; font-weight: bold; }}
                        hr {{ border: none; border-top: 1px solid #ccc; margin: 20px 0; }}
                        p {{ font-size: 14px; line-height: 1.6; margin: 0 0 15px 0; text-align: justify; }}
                        .company-name {{ font-size: 14px; font-weight: bold; margin-top: 30px; }}
                        .legal {{ color: #666; font-size: 11px; line-height: 1.4; margin-top: 20px; }}
                    </style>
                </head>
                <body>
                    <div class=""container"">
                        <div class=""header"">
                            <h1>Su Nota Crédito</h1>
                            <h2>{facturaData.NumeroDocumento}</h2>
                        </div>
                        
                        <hr>
                        
                        <p>Adjunto realizamos envío de la siguiente documentación:</p>
                        
                        <p>
                            Representación Gráfica de la Nota Crédito Electrónica N° {facturaData.NumeroDocumento}.
                        </p>
                        
                        <p>
                            Nota Crédito Electrónica N° {facturaData.NumeroDocumento} por un importe de ${facturaData.Total:N2} de {facturaData.FacturadorNombre}. 
                            Consulte los detalles de plazo y modo de pago en el documento.
                        </p>
                        
                        <p>En caso de requerir cualquier aclaración, por favor no dude en contactarnos.</p>
                        
                        <p>Saludos cordiales,</p>
                        
                        <hr>
                        
                        <p class=""company-name"">{facturaData.FacturadorNombre}</p>
                        
                        <hr>
                        
                        <p class=""legal"">
                            <strong>AVISO:</strong> Este correo electrónico y sus documentos adjuntos son confidenciales y para uso exclusivo 
                            de la persona o entidad a la cual está dirigido. Si no es usted el destinatario, cualquier retención, difusión, 
                            distribución o copia de este mensaje está prohibida.
                        </p>
                    </div>
                </body>
                </html>
            ";
        }
    }
}