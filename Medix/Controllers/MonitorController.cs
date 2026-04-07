using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Medix.Controllers
{
    [Authorize(Roles = "EquipeMedix")]
    public class MonitorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
