using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ServiceStack.Mvc;

namespace Weirdness.Controllers
{
    public class LoginController : ServiceStackController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}