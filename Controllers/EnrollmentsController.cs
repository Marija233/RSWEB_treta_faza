using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Project.Data;
using Project.Models;
using Microsoft.AspNetCore.Authorization;
using Project.ViewModels;

namespace Project.Controllers
{
    public class EnrollmentsController : Controller
    {
        private readonly ProjectContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public EnrollmentsController(ProjectContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Enrollments
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(string SearchString)
        {


            IQueryable<Enrollment> enrollments = _context.Enrollment.AsQueryable();



            if (!string.IsNullOrEmpty(SearchString))
            {
                enrollments = enrollments.Where(s => s.Course.Title.ToLower().Contains(SearchString.ToLower()));
            }

            enrollments = enrollments.Include(s => s.Course).Include(s => s.Student);


            var Title = new EnrollmentTitleViewModel
            {


                Enrollments = await enrollments.ToListAsync()
            };

            return View(Title);
        }

        // GET: Enrollments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var enrollment = await _context.Enrollment
                .Include(e => e.Course)
                .Include(e => e.Student)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (enrollment == null)
            {
                return NotFound();
            }

            return View(enrollment);
        }

        // GET: Enrollments/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["CourseId"] = new SelectList(_context.Course, "Id", "Title");
            ViewData["StudentId"] = new SelectList(_context.Student, "Id", "FullName");
            return View();
        }

        // POST: Enrollments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,CourseId,StudentId,Semester,Year,Grade,SeminarUrl,ProjectUrl,ExamPoints,SeminarPoints,ProjectPoints,AdditionalPoints,FinishDate")] Enrollment enrollment)
         {
             if (ModelState.IsValid)
             {
                 _context.Add(enrollment);
                 await _context.SaveChangesAsync();
                 return RedirectToAction(nameof(Index));
             }
             ViewData["CourseId"] = new SelectList(_context.Course, "Id", "Title", enrollment.CourseId);
             ViewData["StudentId"] = new SelectList(_context.Student, "Id", "FirstName", enrollment.StudentId);
             return View(enrollment);
         }





        // GET: Enrollments/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var enrollment = await _context.Enrollment.FindAsync(id);
            if (enrollment == null)
            {
                return NotFound();
            }
            ViewData["CourseId"] = new SelectList(_context.Course, "Id", "Title", enrollment.CourseId);
            ViewData["StudentId"] = new SelectList(_context.Student, "Id", "FirstName", enrollment.StudentId);
            return View(enrollment);
        }

        // POST: Enrollments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CourseId,StudentId,Semester,Year,Grade,SeminarUrl,ProjectUrl,ExamPoints,SeminarPoints,ProjectPoints,AdditionalPoints,FinishDate")] Enrollment enrollment)
        {
            if (id != enrollment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(enrollment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EnrollmentExists(enrollment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CourseId"] = new SelectList(_context.Course, "Id", "Title", enrollment.CourseId);
            ViewData["StudentId"] = new SelectList(_context.Student, "Id", "FullName", enrollment.StudentId);
            return View(enrollment);
        }

        // GET: Enrollments/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var enrollment = await _context.Enrollment
                .Include(e => e.Course)
                .Include(e => e.Student)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (enrollment == null)
            {
                return NotFound();
            }

            return View(enrollment);
        }


        // POST: Enrollments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var enrollment = await _context.Enrollment.FindAsync(id);
            _context.Enrollment.Remove(enrollment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> StudentsbyCourse(int? id, int enrollmentYear)
        {
            if (id == null)
            {
                return NotFound();
            }

            IQueryable<Enrollment> enrollments = _context.Enrollment.Where(e => e.Id == id);
            enrollments = enrollments.Include(e => e.Course).Include(e => e.Student).OrderBy(e => e.Student.StudentId).OrderBy(e => e.Student.StudentId);

            if (enrollmentYear != 0)
            {
                enrollments = enrollments.Where(x => x.Year == enrollmentYear);
            }
            else
            {

                enrollments = enrollments.Where(x => x.Year == DateTime.Now.Year);
            }


            EnrollmentFilterViewModel vm = new EnrollmentFilterViewModel
            {
                Enrollments = await enrollments.ToListAsync()
            };

            ViewData["currentYear"] = _context.Course.Where(c => c.Id == id).Select(c => c.Title).FirstOrDefault();
            return View(vm);
        }
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> EditByTeacher(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var enrollment = await _context.Enrollment.FindAsync(id);
            if (enrollment == null)
            {
                return NotFound();
            }
            ViewData["CourseId"] = new SelectList(_context.Course, "Id", "Title", enrollment.CourseId);
            ViewData["StudentId"] = new SelectList(_context.Student, "Id", "FirstName", enrollment.StudentId);
            return View(enrollment);
        }

        // POST: Enrollments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> EditByTeacher(int id, [Bind("Id,CourseId,StudentId,Semester,Year,Grade,SeminarUrl,ProjectUrl,ExamPoints,SeminarPoints,ProjectPoints,AdditionalPoints,FinishDate")] Enrollment enrollment)
        {
            if (id != enrollment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(enrollment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EnrollmentExists(enrollment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CourseId"] = new SelectList(_context.Course, "Id", "Title", enrollment.CourseId);
            ViewData["StudentId"] = new SelectList(_context.Student, "Id", "FullName", enrollment.StudentId);
            return View(enrollment);
        }

        [Authorize(Roles = "Student")]
        public async Task<IActionResult> EditByStudent(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var enrollment = await _context.Enrollment.FindAsync(id);
            if (enrollment == null)
            {
                return NotFound();
            }
            ViewData["CourseId"] = new SelectList(_context.Course, "Id", "Title", enrollment.CourseId);
            ViewData["StudentId"] = new SelectList(_context.Student, "Id", "FullName", enrollment.StudentId);
            return View(enrollment);
        }

        // POST: Enrollments/StudentEdit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> EditByStudent(int id, IFormFile pUrl, [Bind("Id,CourseId,StudentId,Semester,Year,Grade,SeminarUrl,ProjectUrl,ExamPoints,SeminarPoints,ProjectPoints,AdditionalPoints,FinishDate")] Enrollment enrollment)
        {
            if (id != enrollment.Id)
            {
                return NotFound();
            }

            EnrollmentsController uploadUrl = new EnrollmentsController(_context, _webHostEnvironment);
            enrollment.SeminarUrl = uploadUrl.UploadedFile(pUrl);


            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(enrollment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EnrollmentExists(enrollment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index", "Enrollments");
            }
            ViewData["CourseId"] = new SelectList(_context.Course, "Id", "Title", enrollment.CourseId);
            ViewData["StudentId"] = new SelectList(_context.Student, "Id", "FullName", enrollment.StudentId);
            return View(enrollment);
        }
        public string UploadedFile(IFormFile file)
        {
            string uniqueFileName = null;
            if (file != null)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "projects");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }
            }
            uniqueFileName = "/projects/" + uniqueFileName;
            return uniqueFileName;
        }

        

        

        public IActionResult CreateEnrollment()
        {
            ViewData["CourseId"] = new SelectList(_context.Course, "Id", "Title");
            ViewData["StudentId"] = new SelectList(_context.Student, "Id", "FullName");
            return View();
        }

     
       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEnrollment([Bind("Id,CourseId,StudentId,Semester,Year")] Enrollment enrollment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(enrollment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(EnrollmentList));
            }
            ViewData["CourseId"] = new SelectList(_context.Course, "Id", "Title", enrollment.CourseId);
            ViewData["StudentId"] = new SelectList(_context.Student, "Id", "FullName", enrollment.StudentId);
            return View(enrollment);
        }

       
       
        public async Task<IActionResult> EnrollmentList()
        {
            var universityContext = _context.Enrollment.Include(e => e.Course).Include(e => e.Student);
            return View(await universityContext.ToListAsync());
        }

        public async Task<IActionResult> ViewWithoutEditing(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var enrollment = await _context.Enrollment.FindAsync(id);
            if (enrollment == null)
            {
                return NotFound();
            }
            ViewData["CourseId"] = new SelectList(_context.Course, "Id", "Title", enrollment.CourseId);
            ViewData["StudentId"] = new SelectList(_context.Student, "Id", "FirstName", enrollment.StudentId);
            return View(enrollment);
        }

        // POST: Enrollments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ViewWithoutEditing(int id, [Bind("Id,CourseId,StudentId,Semester,Year,Grade,SeminarUrl,ProjectUrl,ExamPoints,SeminarPoints,ProjectPoints,AdditionalPoints,FinishDate")] Enrollment enrollment)
        {
            if (id != enrollment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(enrollment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EnrollmentExists(enrollment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CourseId"] = new SelectList(_context.Course, "Id", "Title", enrollment.CourseId);
            ViewData["StudentId"] = new SelectList(_context.Student, "Id", "FullName", enrollment.StudentId);
            return View(enrollment);
        }
        private bool EnrollmentExists(int id)
        {
            return _context.Enrollment.Any(e => e.Id == id);
        }

        

    }


}


