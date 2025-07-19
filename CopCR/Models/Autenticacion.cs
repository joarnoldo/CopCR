using System;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace CopCR.Models
{
    public class Autenticacion
    {
        [Required]
        [Display(Name = "Cédula de Identidad")]
        public string CedulaIdentidad { get; set; }

        [Required]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [Required]
        [Display(Name = "Primer Apellido")]
        public string PrimerApellido { get; set; }

        [Required]
        [Display(Name = "Segundo Apellido")]
        public string SegundoApellido { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Nombre de Usuario")]
        public string NombreUsuario { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Contrasena { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Nacimiento")]
        public DateTime FechaNacimiento { get; set; }

        [Required]
        [Display(Name = "Teléfono de Contacto")]
        public string TelefonoContacto { get; set; }

        [Display(Name = "Foto de Perfil")]
        public HttpPostedFileBase FotoPerfilUrl { get; set; }
    }
}
