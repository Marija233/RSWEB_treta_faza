using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Project.Data;
using Project.Models;
using Project.ViewModels;

namespace Project.Controllers
{
    public class StudentsController : Controller
    {
        private readonly ProjectContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public StudentsController(ProjectContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;

        }

        // GET: Students
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(string StudentIndexSearch, string SearchString)
        {
            IQueryable<Student> students = _context.Student.AsQueryable();
            IQueryable<Student> indexQuery = _context.Student.AsQueryable();
            

           

            if (!string.IsNullOrEmpty(StudentIndexSearch))
            {
               
                students = students.Where(s => s.StudentId.ToLower().Contains(StudentIndexSearch.ToLower()));
            }

            IEnumerable<Student> dataList = students as IEnumerable<Student>;

            if (!string.IsNullOrEmpty(SearchString))
            {
                dataList = dataList.ToList().Where(s => (s.FullName + " " + s.LastName).ToLower().Contains(SearchString.ToLower()));
            }
            students = students.Include(s => s.Courses).ThenInclude(s => s.Course);
            var StudentFullNameStudentId = new StudentFullNameStudentIdViewModel
            {
                Indexes = students.ToList(),
                Students = dataList.ToList()
            };

            return View(StudentFullNameStudentId);
        }

        // GET: Students/Details/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Student.Include(s => s.Courses).ThenInclude(s => s.Course).FirstOrDefaultAsync(m => m.Id == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // GET: Students/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Students/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        /* [HttpPost]
         [ValidateAntiForgeryToken]
         public async Task<IActionResult> Create([Bind("Id,ProfilePicture,StudentId,FirstName,LastName,EnrollmentDate,AcquiredCredits,CurrentSemestar,EducationLevel")] Student student)
         {
             if (ModelState.IsValid)
             {
                 _context.Add(student);
                 await _context.SaveChangesAsync();
                 return RedirectToAction(nameof(Index));
             }
             return View(student);
         }*/

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(StudentFormVM Vmodel)
        {
            if (ModelState.IsValid)
            {
                string uniqueFileName = UploadedFile(Vmodel);

                Student student = new Student
                {
                    ProfilePicture = uniqueFileName,
                    StudentId = Vmodel.StudentId,
                    FirstName = Vmodel.FirstName,
                    LastName = Vmodel.LastName,
                    EnrollmentDate = Vmodel.EnrollmentDate,
                    AcquiredCredits = Vmodel.AcquiredCredits,
                    CurrentSemestar = Vmodel.CurrentSemestar,
                    EducationLevel = Vmodel.EducationLevel,
                    Courses = Vmodel.Courses,
                };

                _context.Add(student);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View();
        }
        private string UploadedFile(StudentFormVM model)
        {
            string uniqueFileName = null;

            if (model.ProfilePicture != null)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.ProfilePicture.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    model.ProfilePicture.CopyTo(fileStream);
                }
            }
            return uniqueFileName;
        }



        // GET: Students/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Student.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }
            StudentFormVM Vmodel = new StudentFormVM
            {
                Id = student.Id,
                FirstName = student.FirstName,
                LastName = student.LastName,
                StudentId = student.StudentId,
                EnrollmentDate = student.EnrollmentDate,
                AcquiredCredits = student.AcquiredCredits,
                CurrentSemestar = student.CurrentSemestar,
                EducationLevel = student.EducationLevel,
                Courses = student.Courses
            };
            ViewData["StudentFullName"] = _context.Student.Where(t => t.Id == id).Select(t => t.FullName).FirstOrDefault();

            return View(Vmodel);
       
        }

        // POST: Students/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        /*  [HttpPost]
          [ValidateAntiForgeryToken]
          public async Task<IActionResult> Edit(int id, [Bind("Id,ProfilePicture,StudentId,FirstName,LastName,EnrollmentDate,AcquiredCredits,CurrentSemestar,EducationLevel")] Student student)
          {
              if (id != student.Id)
              {
                  return NotFound();
              }

              if (ModelState.IsValid)
              {
                  try
                  {
                      _context.Update(student);
                      await _context.SaveChangesAsync();
                  }
                  catch (DbUpdateConcurrencyException)
                  {
                      if (!StudentExists(student.Id))
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
              return View(student);
          }*/
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Edit(int id, StudentFormVM Vmodel)
        {
            if (id != Vmodel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    string uniqueFileName = UploadedFile(Vmodel);

                    Student student = new Student
                    {
                        Id = Vmodel.Id,
                        FirstName = Vmodel.FirstName,
                        LastName = Vmodel.LastName,
                        ProfilePicture = uniqueFileName,
                        EnrollmentDate = Vmodel.EnrollmentDate,
                        CurrentSemestar = Vmodel.CurrentSemestar,
                        AcquiredCredits = Vmodel.AcquiredCredits,
                        StudentId = Vmodel.StudentId,
                        EducationLevel = Vmodel.EducationLevel,
                        Courses = Vmodel.Courses
                    };

                    _context.Update(student);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentExists(Vmodel.Id))
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
            return View(Vmodel);
        }

        // GET: Students/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Student.Include(s => s.Courses).ThenInclude(s => s.Course).FirstOrDefaultAsync(m => m.Id == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _context.Student.FindAsync(id);

           
            string path = Path.Combine(_webHostEnvironment.WebRootPath, "images", student.ProfilePicture);
            FileInfo file = new FileInfo(path);
            if (file.Exists)
            {
                file.Delete();
            }

            _context.Student.Remove(student);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StudentExists(int id)
        {
            return _context.Student.Any(e => e.Id == id);
        }
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> MyCourses(long? id)
        {
            IQueryable<Course> courses = _context.Course.Include(c => c.FirstTeacher).Include(c => c.SecondTeacher).AsQueryable();

            IQueryable<Enrollment> enrollments = _context.Enrollment.AsQueryable();
            enrollments = enrollments.Where(s => s.StudentId == id); 
            IEnumerable<int> enrollmentsIDS = enrollments.Select(e => e.CourseId).Distinct(); 

            courses = courses.Where(s => enrollmentsIDS.Contains(s.Id));  

            courses = courses.Include(c => c.Students).ThenInclude(c => c.Student);

            ViewData["StudentName"] = _context.Student.Where(t => t.Id == id).Select(t => t.FullName).FirstOrDefault();
            ViewData["studentId"] = id;
            return View(courses);
        }

    }
}
