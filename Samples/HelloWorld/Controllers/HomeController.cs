using Microsoft.AspNetCore.Mvc;
using System.Dynamic;

namespace HelloWorld.Controllers
{
    public class HomeController : Controller
    {
        private readonly OpenTokService _opentokService;

        public HomeController(OpenTokService opentokService)
        {
            _opentokService = opentokService;
        }

        public IActionResult Index()
        {
            dynamic locals = new ExpandoObject();

            locals.ApiKey = _opentokService.OpenTok.ApiKey.ToString();
            locals.SessionId = _opentokService.Session.Id;
            locals.Token = _opentokService.Session.GenerateToken();

            return View(locals);  
        }
    }

}
