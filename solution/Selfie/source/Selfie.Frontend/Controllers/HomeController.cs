using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Selfie.Frontend.Models;

namespace Selfie.Frontend.Controllers
{
    public class HomeController : Controller
    {
        #region MEMBERS
        string backend;
        #endregion

        #region CONSTRUCTORS
        public HomeController(IConfiguration configuration)
        {
            backend = configuration["Services:Backend"];
        }
        #endregion

        #region CONTROLLERS
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet()]
        [Route("/config")]
        public IActionResult services()
        {
            var config = new Dictionary<string, string>() {
                { "backend", backend }
            };           

            return Ok(config);
        }
        #endregion
    }
}
