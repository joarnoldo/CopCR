using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CopCR.EF;
using CopCR.Models;
using CopCR.Services;

namespace CopCR.Controllers
{
    [FiltroSesion]
    public class UsuarioController : Controller
    {
        private readonly Utilitarios service = new Utilitarios();

        #region Consultar Perfil

        [HttpGet]
        public ActionResult ConsultarPerfilUsuario()
        {
            var model = new UsuarioModel();
            int idUsuario = int.Parse(Session["IdUsuario"].ToString());

            using (var db = new CopCR_DevEntities())
            {
                // Invoca el SP y mapea al model
                var datos = db.Database
                    .SqlQuery<UsuarioModel>(
                        "EXEC dbo.ConsultarPerfilUsuario @UsuarioID",
                        new SqlParameter("@UsuarioID", idUsuario)
                    )
                    .FirstOrDefault();

                if (datos != null)
                {
                    model = datos;
                    Session["Nombre"] = $"{model.Nombre} {model.PrimerApellido}";
                    Session["FotoPerfilUrl"] = model.FotoPerfilUrl;
                    Session["NombreUsuario"] = model.NombreUsuario;
                }
            }

            return View(model);
        }

        #endregion

        #region Actualizar Perfil

        [HttpPost]
        public ActionResult ActualizarPerfilUsuario(UsuarioModel usuario, HttpPostedFileBase foto)
        {
            int idUsuario = int.Parse(Session["IdUsuario"].ToString());
            ViewBag.Mensaje = "No se pudo actualizar la información";

            // Guarda la URL de la foto si hay
            string nuevaRuta = usuario.FotoPerfilUrl;
            if (foto != null && foto.ContentLength > 0)
            {
                string nombreArchivo = $"perfil_{idUsuario}{System.IO.Path.GetExtension(foto.FileName)}";
                string carpeta = Server.MapPath("~/Uploads/Perfiles/");
                System.IO.Directory.CreateDirectory(carpeta);
                string rutaCompleta = System.IO.Path.Combine(carpeta, nombreArchivo);
                foto.SaveAs(rutaCompleta);
                nuevaRuta = "/Uploads/Perfiles/" + nombreArchivo;
            }

            using (var db = new CopCR_DevEntities())
            {
                // Llama al SP para actualizar
                int filas = db.Database.ExecuteSqlCommand(
                    "EXEC dbo.ActualizarPerfilUsuario " +
                    "@UsuarioID, @Nombre, @PrimerApellido, @SegundoApellido, @Email, @NombreUsuario, " +
                    "@TelefonoContacto, @FechaNacimiento, @FotoPerfilUrl, @AceptaNotificacionesPush",
                    new SqlParameter("@UsuarioID", idUsuario),
                    new SqlParameter("@Nombre", usuario.Nombre),
                    new SqlParameter("@PrimerApellido", usuario.PrimerApellido),
                    new SqlParameter("@SegundoApellido", usuario.SegundoApellido),
                    new SqlParameter("@Email", usuario.Email),
                    new SqlParameter("@NombreUsuario", usuario.NombreUsuario),
                    new SqlParameter("@TelefonoContacto", (object)usuario.TelefonoContacto ?? DBNull.Value),
                    new SqlParameter("@FechaNacimiento", (object)usuario.FechaNacimiento ?? DBNull.Value),
                    new SqlParameter("@FotoPerfilUrl", (object)nuevaRuta ?? DBNull.Value),
                    new SqlParameter("@AceptaNotificacionesPush", usuario.AceptaNotificacionesPush)
                );

                if (filas > 0)
                {
                    ViewBag.Mensaje = "Información actualizada correctamente";
                    Session["Nombre"] = $"{usuario.Nombre} {usuario.PrimerApellido}";
                    Session["FotoPerfilUrl"] = nuevaRuta;
                }
            }

            usuario.FotoPerfilUrl = nuevaRuta;
            return View("ConsultarPerfilUsuario", usuario);
        }

        #endregion

        #region Cambiar Contraseña

        [HttpPost]
        public ActionResult CambiarContrasenna(UsuarioModel usuario)
        {
            int idUsuario = int.Parse(Session["IdUsuario"].ToString());
            ViewBag.Mensaje = "No se pudo actualizar la contraseña";

            // 1) Verificar la contraseña actual con BCrypt
            using (var db = new CopCR_DevEntities())
            {
                var efUser = db.Usuario.FirstOrDefault(u => u.UsuarioID == idUsuario);
                if (efUser == null || !BCrypt.Net.BCrypt.Verify(usuario.Contrasena, efUser.Contrasena))
                {
                    ViewBag.Mensaje = "La contraseña actual es incorrecta";
                    return View(usuario);
                }
            }

            // 2) Hashear la nueva contraseña
            var hashNuevo = BCrypt.Net.BCrypt.HashPassword(usuario.ContrasenaNueva);

            // 3) Ejecutar el SP dedicado
            int filas;
            using (var db = new CopCR_DevEntities())
            {
                filas = db.Database.ExecuteSqlCommand(
                    "EXEC dbo.CambiarContrasenaUsuario @UsuarioID, @NuevoHash",
                    new SqlParameter("@UsuarioID", idUsuario),
                    new SqlParameter("@NuevoHash", hashNuevo)
                );
            }

            if (filas > 0)
            {
                ViewBag.Mensaje = "Contraseña actualizada correctamente";
                usuario.Contrasena = "";
                usuario.ContrasenaNueva = "";
            }

            return View(usuario);
        }


        #endregion
    }
}


