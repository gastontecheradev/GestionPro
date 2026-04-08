using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GestionPro.Filters
{
    /// <summary>
    /// Bloquea operaciones de escritura (Create, Edit, Delete) para usuarios demo.
    /// Los usuarios demo pueden ver todo pero no modificar datos.
    /// Uso: [DemoReadOnly] en acciones o controllers.
    /// </summary>
    public class DemoReadOnlyAttribute : ActionFilterAttribute
    {
        // Emails de las cuentas demo
        private static readonly HashSet<string> DemoEmails = new(StringComparer.OrdinalIgnoreCase)
        {
            "admin@gestionpro.com",
            "vendedor@gestionpro.com"
        };

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userEmail = context.HttpContext.User?.Identity?.Name;

            if (!string.IsNullOrEmpty(userEmail) && DemoEmails.Contains(userEmail))
            {
                // Si es AJAX/JSON, devolver JSON
                if (context.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest"
                    || context.HttpContext.Request.ContentType?.Contains("json") == true)
                {
                    context.Result = new JsonResult(new
                    {
                        error = true,
                        message = "Esta es una demo de solo lectura. Registrate con tu propia cuenta para modificar datos."
                    })
                    {
                        StatusCode = 403
                    };
                    return;
                }

                // Para requests normales, redirigir con mensaje
                if (context.Controller is Controller controller)
                {
                    controller.TempData["Error"] =
                        "🔒 Modo demo: solo lectura. Podés ver todo pero no modificar datos. " +
                        "Registrate con tu propia cuenta para tener acceso completo.";
                }

                // Redirigir a la página anterior o al Index del controller actual
                var referer = context.HttpContext.Request.Headers["Referer"].ToString();
                if (!string.IsNullOrEmpty(referer))
                {
                    context.Result = new RedirectResult(referer);
                }
                else
                {
                    var controllerName = context.RouteData.Values["controller"]?.ToString() ?? "Home";
                    context.Result = new RedirectToActionResult("Index", controllerName, null);
                }
            }
        }
    }
}
