using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Project.Areas.Identity.Data;
using Project.Data;
using Project.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.Controllers
{
    public class AdminController : Controller
    {
        private UserManager<ProjectUser> userManager;
        private RoleManager<IdentityRole> roleManager;
        private readonly ProjectContext _context;
        private IPasswordHasher<ProjectUser> passwordHasher;
        public AdminController(UserManager<ProjectUser> usrMgr, ProjectContext context, RoleManager<IdentityRole> roleMgr, IPasswordHasher<ProjectUser> passwordHash)
        {
            userManager = usrMgr;
            _context = context;
            roleManager = roleMgr;
            passwordHasher = passwordHash;
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            return View(userManager.Users);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["Teachers"] = new SelectList(_context.Set<Teacher>(), "Id", "FullName");
            ViewData["Students"] = new SelectList(_context.Set<Student>(), "Id", "FullName");
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(User user)
        {
            if (ModelState.IsValid)
            {

                ViewData["Teachers"] = new SelectList(_context.Set<Teacher>(), "Id", "FullName", user.TeacherId);
                ViewData["Students"] = new SelectList(_context.Set<Teacher>(), "Id", "FullName", user.StudentId);
                ProjectUser appUser = new ProjectUser
                {
                    UserName = user.UserName,
                    Email = user.Email,
                    StudentId = user.StudentId,
                    TeacherId = user.TeacherId
                };

                IdentityResult result = await userManager.CreateAsync(appUser, user.Password);
                if (result.Succeeded)
                {
                    //Stavi mu Role na korisnikot
                    if (user.TeacherId != null)
                    {
                        var result1 = await userManager.AddToRoleAsync(appUser, "Teacher");
                    }
                    else if (user.StudentId != null)
                    {
                        var result2 = await userManager.AddToRoleAsync(appUser, "Student");
                    }
                    return RedirectToAction("Index");
                }

                else
                {
                    foreach (IdentityError error in result.Errors)
                        ModelState.AddModelError("", error.Description);
                }
            }

            return View(user);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(string id)
        {
            ProjectUser user = await userManager.FindByIdAsync(id);
            if (user != null)
                return View(user);
            else
                return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(string id, string email, string password)
        {
            ProjectUser user = await userManager.FindByIdAsync(id);
            if (user != null)
            {
                if (!string.IsNullOrEmpty(email))
                    user.Email = email;
                else
                    ModelState.AddModelError("", "Email cannot be empty");

                if (!string.IsNullOrEmpty(password))
                    user.PasswordHash = passwordHasher.HashPassword(user, password);
                else
                    ModelState.AddModelError("", "Password cannot be empty");

                if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
                {
                    IdentityResult result = await userManager.UpdateAsync(user);
                    if (result.Succeeded)
                        return RedirectToAction("Index");
                    else
                        Errors(result);
                }
            }
            else
                ModelState.AddModelError("", "User Not Found");
            return View(user);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            ProjectUser user = await userManager.FindByIdAsync(id);
            if (user != null)
            {
                IList<String> list = await userManager.GetRolesAsync(user);
                //Admins ne mozhat da se izbrishat
                if (!list.Contains("Admin"))
                {
                    IdentityResult result = await userManager.DeleteAsync(user);
                    if (result.Succeeded)
                        return RedirectToAction("Index");
                    else
                        Errors(result);
                }
            }
            else
                ModelState.AddModelError("", "User Not Found");
            return View("Index", userManager.Users);
        }

        private void Errors(IdentityResult result)
        {
            foreach (IdentityError error in result.Errors)
                ModelState.AddModelError("", error.Description);
        }
    }
}

