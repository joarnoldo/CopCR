using System;
using System.Web;

namespace CopCR.Models
{
    public class UsuarioModel
    {
        // Sólo lectura
        public string Identificacion { get; set; }
        public int PuntosConfianza { get; set; }
        public DateTime FechaRegistro { get; set; }

        // Editables
        public string Nombre { get; set; }
        public string PrimerApellido { get; set; }
        public string SegundoApellido { get; set; }
        public string Email { get; set; }
        public string NombreUsuario { get; set; }
        public string TelefonoContacto { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public bool AceptaNotificacionesPush { get; set; }

        // Para subir la foto
        public string FotoPerfilUrl { get; set; }
        public HttpPostedFileBase FotoFile { get; set; }

        // Cambio de contraseña
        public string Contrasena { get; set; }
        public string ContrasenaNueva { get; set; }
    }
}