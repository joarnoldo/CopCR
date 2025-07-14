using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KProyecto.Models
{
    public class FiltroSesion : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var contexto = filterContext.HttpContext;

            if (contexto.Session.Count == 0)
            {
                // Redireccionarlo a la pantalla de inicio
                filterContext.Result = new RedirectResult("~/Home/Index");
            }

            base.OnActionExecuting(filterContext);
        }
    }

    public class FiltroAdministrador : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var contexto = filterContext.HttpContext;

            if (contexto.Session["IdRol"].ToString() != "2")
            {
                // Redireccionarlo a la pantalla de inicio
                filterContext.Result = new RedirectResult("~/Home/Principal");
            }

            base.OnActionExecuting(filterContext);
        }
    }
}