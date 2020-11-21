using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Auth.Controllers
{
    [Route("api/[controller]s")]
    public class AuthController : ControllerBase
    {
        [Route("healthz")]
        [HttpGet]
        public string Healthz()
        {
            return "AuthService active";
        }
    }
}
