using BCrypt.Net;
using CopCR.EF;
using CopCR.Models;
using CopCR.Services;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web.Mvc;

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
                //Buscar usuario activo por cédula
                var user = db.Usuario
                             .FirstOrDefault(u =>
                                 u.CedulaIdentidad == autenticacion.CedulaIdentidad &&
                                 u.Activo);

                //Verificar contraseña con BCrypt
                if (user != null && BCrypt.Net.BCrypt.Verify(autenticacion.Contrasena, user.Contrasena))
                {
                    //Guardar datos en sesión
                    Session["IdUsuario"] = user.UsuarioID;
                    Session["Nombre"] = $"{user.Nombre} {user.PrimerApellido}";
                    //Detectar si es admin o usuario final
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
            var hash = BCrypt.Net.BCrypt.HashPassword(autenticacion.Contrasena);

            //Parámetros
            var pCedula = new System.Data.SqlClient.SqlParameter("@CedulaIdentidad", autenticacion.CedulaIdentidad);
            var pNombre = new System.Data.SqlClient.SqlParameter("@Nombre", autenticacion.Nombre);
            var pPriApe = new System.Data.SqlClient.SqlParameter("@PrimerApellido", autenticacion.PrimerApellido);
            var pSegApe = new System.Data.SqlClient.SqlParameter("@SegundoApellido", autenticacion.SegundoApellido);
            var pEmail = new System.Data.SqlClient.SqlParameter("@Email", autenticacion.Email);
            var pUsr = new System.Data.SqlClient.SqlParameter("@NombreUsuario", autenticacion.NombreUsuario);
            var pHash = new System.Data.SqlClient.SqlParameter("@Contrasena", hash);
            var pFechaNac = new System.Data.SqlClient.SqlParameter("@FechaNacimiento", autenticacion.FechaNacimiento);
            var pTelefono = new System.Data.SqlClient.SqlParameter("@TelefonoContacto", autenticacion.TelefonoContacto);
            var pFoto = new SqlParameter("@FotoPerfilUrl", (object)autenticacion.FotoPerfilUrl ?? DBNull.Value);

            using (var db = new CopCR_DevEntities())
            {

            var sql = @" DECLARE @ReturnValue INT;
            EXEC @ReturnValue = dbo.RegistroUsuario 
                 @CedulaIdentidad,
                 @Nombre,
                 @PrimerApellido,
                 @SegundoApellido,
                 @Email,
                 @NombreUsuario,
                 @Contrasena,
                 @FechaNacimiento,
                 @TelefonoContacto,
                 @FotoPerfilUrl;
            SELECT @ReturnValue;";

                var result = db.Database.SqlQuery<int>(
                    sql,
                    pCedula, pNombre, pPriApe, pSegApe,
                    pEmail, pUsr, pHash, pFechaNac, pTelefono, pFoto
                ).Single();

                //Mensajes del código devuelto
                if (result == 0)
                    return RedirectToAction("Login", "Home");

                switch (result)
                {
                    case -2:
                        ViewBag.Mensaje = "Ya existe un usuario con ese correo.";
                        break;
                    case -3:
                        ViewBag.Mensaje = "Ya existe un usuario con esa cédula.";
                        break;
                    default:
                        ViewBag.Mensaje = $"No se pudo registrar su información (código {result}).";
                        break;
                }
            }

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


