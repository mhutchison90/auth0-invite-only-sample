using System.Linq;
using System.Security.Claims;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace Analystick.Web.Controllers
{
    [Authorize(Roles = "Member")]
    public class ProfileController : Controller
    {
        public ActionResult Index()
        {
            return View("Index", (object)JsonConvert.SerializeObject((User.Identity as ClaimsIdentity)
                .Claims.Select(c => new {type = c.Type, value = c.Value}).OrderBy(c => c.type), Formatting.Indented));
        }
    }
}