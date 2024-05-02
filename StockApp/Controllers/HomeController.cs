using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace StockApp.Controllers
{
    //to make the controller available for people who didn't signin
    [AllowAnonymous]
    [Route("[Controller]")]
    public class HomeController : Controller
    {
        [Route("[Action]")]
        public IActionResult Error()
        {
            ViewBag.path = "Error";
            ViewBag.dep = "Home";
            var exceptionHandler = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            if (exceptionHandler != null)
            {
                ViewBag.errorMessage = exceptionHandler.Error.Message;
            }
            return View();
        }
    }
}
