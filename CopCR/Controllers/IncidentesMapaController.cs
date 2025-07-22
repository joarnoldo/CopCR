using CopCR.EF;
using CopCR.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace CopCR.Controllers
{
    [FiltroSesion]
    public class IncidentesMapaController : Controller
    {

        [HttpGet]
        public ActionResult MapaUsuario()
        {
            return View();
        }

        //Obtener Ubicacion Principal
        [HttpGet]
        public JsonResult GetDireccionPrincipal()
        {
            int userId = int.Parse(Session["IdUsuario"].ToString());
            using (var db = new CopCR_DevEntities())
            {
                var dir = db.Direccion
                            .Where(d => d.UsuarioID == userId && d.IsDomicilioPrincipal)
                            .Select(d => new {
                                d.Latitud,
                                d.Longitud
                            })
                            .FirstOrDefault();

                if (dir == null)
                    return Json(new { success = false }, JsonRequestBehavior.AllowGet);

                return Json(new
                {
                    success = true,
                    lat = dir.Latitud,
                    lng = dir.Longitud
                }, JsonRequestBehavior.AllowGet);
            }
        }


        //Si es principal, desmarca la anterior.
        [HttpPost]
        public JsonResult SaveDireccion(double Latitud, double Longitud, bool IsDomicilioPrincipal)
        {
            int userId = int.Parse(Session["IdUsuario"].ToString());
            using (var db = new CopCR_DevEntities())
            {
                if (IsDomicilioPrincipal)
                {
                    // Desactiva el anterior
                    var prev = db.Direccion
                                 .FirstOrDefault(d => d.UsuarioID == userId && d.IsDomicilioPrincipal);
                    if (prev != null)
                    {
                        // Actualiza coordenadas
                        prev.Latitud = (decimal)Latitud;
                        prev.Longitud = (decimal)Longitud;
                    }
                    else
                    {
                        // Crea nuevo
                        db.Direccion.Add(new Direccion
                        {
                            UsuarioID = userId,
                            Latitud = (decimal)Latitud,
                            Longitud = (decimal)Longitud,
                            IsDomicilioPrincipal = true
                        });
                    }
                }
                else
                {

                    db.Direccion.Add(new Direccion
                    {
                        UsuarioID = userId,
                        Latitud = (decimal)Latitud,
                        Longitud = (decimal)Longitud,
                        IsDomicilioPrincipal = false
                    });
                }

                db.SaveChanges();
                return Json(new { success = true });
            }
        }

        //Devuelve todos los incidentes para pintar en el mapa
        [HttpGet]
        public JsonResult GetIncidentes()
        {
            using (var db = new CopCR_DevEntities())
            {
                var list = db.Incidente
                             .Select(i => new {
                                 i.IncidenteID,
                                 i.Latitud,
                                 i.Longitud,
                                 i.UsuarioID,
                                 i.Titulo,
                                 i.Descripcion,
                                 i.CategoriaIncidenteID,
                                 i.EstadoId,
                                 FechaReporte = i.FechaReporte
                             })
                             .ToList();

                return Json(list, JsonRequestBehavior.AllowGet);
            }
        }

        //Devuelve todas las categorías de incidente
        [HttpGet]
        public JsonResult GetCategorias()
        {
            using (var db = new CopCR_DevEntities())
            {
                var cats = db.CategoriaIncidente
                             .Select(c => new {
                                 c.CategoriaIncidenteID,
                                 c.Nombre
                             })
                             .ToList();

                return Json(cats, JsonRequestBehavior.AllowGet);
            }
        }
    }
}

