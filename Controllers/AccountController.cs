using Microsoft.AspNetCore.Mvc;

namespace BuildingManagment.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login()
        {
            
            return View();
        }
        [HttpPost]
        public IActionResult Login(string username, string password)
        {  
            HttpContext.Session.SetString("username", username);
            

            return RedirectToAction("index","home");
        }
        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("index", "home");
        }

    }
}
