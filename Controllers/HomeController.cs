using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Project.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Principal;
using Project.Data;
using Microsoft.AspNetCore.Identity;
using Project.Areas.Identity.Data;

namespace Project.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ProjectContext _context;
        private readonly UserManager<ProjectUser> userManager;

        public HomeController(ILogger<HomeController> logger, ProjectContext context, UserManager<ProjectUser> usrMgr)
        {
            _logger = logger;
            _context = context;
            userManager = usrMgr;
        }

        public async Task<IActionResult> Index()
        {
            return View();
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Courses");
            }
            else if (User.IsInRole("Teacher"))
            {
                //Get TeacherId
                var userID = userManager.GetUserId(User);
                ProjectUser user = await userManager.FindByIdAsync(userID);
                return RedirectToAction("Courses", "Teachers", new { id = user.TeacherId });
            }
            else if (User.IsInRole("Student"))
            {
                var userID = userManager.GetUserId(User);
                ProjectUser user = await userManager.FindByIdAsync(userID);
                return RedirectToAction("EnrollmentList", "Enrollments", new { id = user.StudentId });
            }
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
