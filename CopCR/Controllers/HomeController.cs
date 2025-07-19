using CopCR.Models;
using CopCR.Services;
using KProyecto.Models;
using System;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace CopCR.Controllers
{
    public class HomeController : Controller
    {
        readonly Utilitarios service = new Utilitarios();

        #region Index (Login)

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(Autenticacion autenticacion)
        {
            using (var dbContext = new CopCR_DevEntities1())
            {
                var result = dbContext.ValidarInicioSesion(
                    autenticacion.CedulaIdentidad,
                    autenticacion.Contrasena).FirstOrDefault();

                if (result != null)
                {
                    Session["IdUsuario"] = result.IdUsuario;
                    Session["Nombre"] = result.Nombre;
                    Session["IdRol"] = result.IdRol;
                    Session["DescripcionRol"] = result.DescripcionRol;

                    return RedirectToAction("Index", "Home");
                }

                ViewBag.Mensaje = "No se pudo validar su información";
                return View("Login");
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
            using (var dbContext = new CopCR_DevEntities1())
            {
                var result = dbContext.RegistroUsuario(
                    autenticacion.CedulaIdentidad,
                    autenticacion.Nombre,
                    autenticacion.Email,
                    autenticacion.Contrasena);

                if (result > 0)
                    return RedirectToAction("Index", "Home");

                ViewBag.Mensaje = "No se pudo registrar su información";
                return View();
            }
        }

        #endregion

        #region RecuperarContrasena

        [HttpGet]
        public ActionResult RecuperarContrasena()
        {
            return View();
        }

        [HttpPost]
        public ActionResult RecuperarContrasena(Autenticacion autenticacion)
        {
            using (var dbContext = new CopCR_DevEntities1())
            {
                var result = dbContext.TUsuario
                    .FirstOrDefault(u => u.Email == autenticacion.Email);

                if (result != null)
                {
                    var nuevaContrasena = service.GenerarPassword();
                    result.Contrasena = nuevaContrasena;
                    dbContext.SaveChanges();

                    StringBuilder mensaje = new StringBuilder();

                    mensaje.Append("Estimado/a " + result.Nombre + "<br>");
                    mensaje.Append("Se ha generado una solicitud de recuperación de contraseña.<br><br>");
                    mensaje.Append("Su contraseña temporal es: <strong>" + nuevaContrasena + "</strong><br><br>");
                    mensaje.Append("Por favor cambie su contraseña después de iniciar sesión.<br>");
                    mensaje.Append("Gracias por usar CopCR.");

                    if (service.EnviarCorreo(result.Email, mensaje.ToString(), "Recuperación de Acceso"))
                        return RedirectToAction("Index", "Home");

                    ViewBag.Mensaje = "No se pudo enviar la notificación de acceso al correo";
                    return View();
                }

                ViewBag.Mensaje = "No se encontró un usuario con ese correo";
                return View();
            }
        }

        #endregion

        #region Sesión y vista Index

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
            return RedirectToAction("Index", "Home");
        }

        #endregion
    }
}
