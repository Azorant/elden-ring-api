using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace EldenRingAPI.Controllers
{
    public class BaseController : Controller
    {
        [NonAction]
        public JsonResult HandleResponse(Response response)
        {
            Response.StatusCode = (int)response.StatusCode;
            return Json(response);
        }
    }
}
