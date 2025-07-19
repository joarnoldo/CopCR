using System;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using BCrypt.Net;
using CopCR.EF;
using CopCR.Models;
using CopCR.Services;

namespace CopCR.Controllers
{
    public class HomeController : Controller
    {
        readonly Utilitarios service = new Utilitarios();

        #region Login

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(Autenticacion autenticacion)
        {
            using (var db = new CopCR_DevEntities())
            {
                // 1) Buscar usuario activo por cédula
                var user = db.Usuario
                             .FirstOrDefault(u =>
                                 u.CedulaIdentidad == autenticacion.CedulaIdentidad &&
                                 u.Activo);

                // 2) Verificar contraseña con BCrypt
                if (user != null && BCrypt.Net.BCrypt.Verify(autenticacion.Contrasena, user.Contrasena))
                {
                    // 3) Guardar datos en sesión
                    Session["IdUsuario"] = user.UsuarioID;
                    Session["Nombre"] = $"{user.Nombre} {user.PrimerApellido}";
                    // 4) Detectar si es admin o usuario final
                    bool esAdmin = db.Administrador.Any(a => a.UsuarioID == user.UsuarioID);
                    Session["IdRol"] = esAdmin ? "ADMIN" : "USER";
                    Session["DescripcionRol"] = esAdmin ? "Administrador" : "UsuarioFinal";

                    return RedirectToAction("Index", "Home");
                }

                ViewBag.Mensaje = "Cédula o contraseña incorrectos";
                return View();
            }
        }

        #endregion

        #region Registro

        [HttpGet]
        public ActionResult RegistroUsuario()
        {
            return View();
        }

        [HttpPost]
        public ActionResult RegistroUsuario(Autenticacion autenticacion)
        {
            if (!ModelState.IsValid)
                return View(autenticacion);

            // 1) Hashear contraseña
            string hash = BCrypt.Net.BCrypt.HashPassword(autenticacion.Contrasena);

            // 2) Llamar al SP RegistroUsuario (incluye Usuario y UsuarioFinal)
            using (var db = new CopCR_DevEntities())
            {
                var result = db.RegistroUsuario(
                    autenticacion.CedulaIdentidad,
                    autenticacion.Nombre,
                    autenticacion.PrimerApellido,
                    autenticacion.SegundoApellido,
                    autenticacion.Email,
                    autenticacion.NombreUsuario,
                    hash,
                    autenticacion.FechaNacimiento,
                    autenticacion.TelefonoContacto,
                    null 
                );

                if (result == 0)
                    return RedirectToAction("Login", "Home");
            }

            ViewBag.Mensaje = "No se pudo registrar su información";
            return View(autenticacion);
        }

        #endregion

        #region Recuperar contraseña

        [HttpGet]
        public ActionResult RecuperarContrasena()
        {
            return View();
        }

        [HttpPost]
        public ActionResult RecuperarContrasena(Autenticacion autenticacion)
        {
            using (var db = new CopCR_DevEntities())
            {
                var user = db.Usuario
                             .FirstOrDefault(u => u.Email == autenticacion.Email);

                if (user != null)
                {
                    // Generar y hashear nueva contraseña
                    var nuevaContrasena = service.GenerarPassword();
                    user.Contrasena = BCrypt.Net.BCrypt.HashPassword(nuevaContrasena);
                    db.SaveChanges();

                    var sb = new StringBuilder();
                    sb.Append($"Estimado/a {user.Nombre}<br>");
                    sb.Append("Se ha generado una solicitud de recuperación de contraseña.<br><br>");
                    sb.Append($"Su contraseña temporal es: <strong>{nuevaContrasena}</strong><br><br>");
                    sb.Append("Por favor cambie su contraseña después de iniciar sesión.<br>");
                    sb.Append("Gracias por usar CopCR.");

                    if (service.EnviarCorreo(user.Email, sb.ToString(), "Recuperación de Acceso"))
                        return RedirectToAction("Login", "Home");

                    ViewBag.Mensaje = "No se pudo enviar la notificación al correo";
                    return View();
                }

                ViewBag.Mensaje = "No se encontró un usuario con ese correo";
                return View();
            }
        }

        #endregion

        #region Área protegida

        [FiltroSesion]
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [FiltroSesion]
        [HttpGet]
        public ActionResult CerrarSesion()
        {
            Session.Clear();
            return RedirectToAction("Login", "Home");
        }

        #endregion
    }
}


