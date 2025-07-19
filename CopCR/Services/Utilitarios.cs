﻿using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace CopCR.Services
{
    public class Utilitarios
    {
        public bool EnviarCorreo(string destinatario, string mensaje, string asunto)
        {
            try
            {
                var remitente = ConfigurationManager.AppSettings["CorreoRemitente"];
                var contrasena = ConfigurationManager.AppSettings["CorreoPassword"];

                MailMessage mail = new MailMessage
                {
                    From = new MailAddress(remitente),
                    Subject = asunto,
                    Body = mensaje,
                    IsBodyHtml = true
                };
                mail.To.Add(destinatario);

                SmtpClient smtp = new SmtpClient("smtp.office365.com", 587)
                {
                    Credentials = new NetworkCredential(remitente, contrasena),
                    EnableSsl = true
                };

                smtp.Send(mail);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string GenerarPassword(int longitud = 8)
        {
            const string caracteres = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var sb = new StringBuilder(longitud);

            for (int i = 0; i < longitud; i++)
            {
                int index = random.Next(caracteres.Length);
                sb.Append(caracteres[index]);
            }

            return sb.ToString();
        }
    }
}
