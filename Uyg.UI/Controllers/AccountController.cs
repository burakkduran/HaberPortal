using Microsoft.AspNetCore.Mvc;
using Uyg.UI.Models;

namespace Uyg.UI.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }
    }
} 