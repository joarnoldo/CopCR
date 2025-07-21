using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;

namespace CopCR.Models
{
    public class FiltroSesion : ActionFilterAttribute
    {
        // Lista de acciones públicas que no llevan sesión
        private static readonly string[] AccionesPublicas = new[]
        {
            "Login", "RegistroUsuario", "RecuperarContrasena"
        };

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var session = filterContext.HttpContext.Session;
            var action = filterContext.ActionDescriptor.ActionName;

            // Si es una acción pública, no hacemos nada
            if (AccionesPublicas.Contains(action))
            {
                base.OnActionExecuting(filterContext);
                return;
            }

            // Si no hay IdUsuario en sesión, redirigimos a Login
            if (session["IdUsuario"] == null)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary {
                        { "controller", "Home" },
                        { "action", "Login" }
                    }
                );
                return;
            }

            base.OnActionExecuting(filterContext);
        }
    }

    public class FiltroAdministrador : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var session = filterContext.HttpContext.Session;
            var role = session["IdRol"] as string;

            if (role == null || role != "ADMIN")
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary {
                        { "controller", "Home" },
                        { "action", "Index" }
                    }
                );
                return;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
