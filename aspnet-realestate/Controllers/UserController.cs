using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace aspnet_realestate.Controllers
{
    [Authorize(Roles = "User")]
    public class UserController : Controller
    {
        /*
         Buraya birde user/kullanici_adi ekleyecez herkese açık profiller için
         */
        [Route("user/dashboard")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("user/settings")]
        public IActionResult Settings()
        {
            return View();
        }

        [Route("user/properties")]
        public IActionResult Properties()
        {
            return View();
        }

        [Route("user/favorites")]
        public IActionResult Favorites()
        {
            return View();
        }

        [Route("user/messages")]
        public IActionResult Messages()
        {
            return View();
        }
    }
}
