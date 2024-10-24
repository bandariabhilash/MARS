using DataAccess.Db;
using Microsoft.AspNetCore.Mvc;

namespace ServiceApis.Controllers
{
    public class BaseController : Controller
    {
        protected FBContext context = new FBContext();
    }
}
