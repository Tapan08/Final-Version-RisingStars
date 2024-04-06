using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RisingStars.Models;
using RisingStarsData.DataAccess;
using RisingStarsData.Entities;

namespace RisingStars.Controllers
{
    public class ProjectController : Controller
    {

        private readonly AppDbContext _context;
        private readonly UserManager<Student> _userManager;

        // host environment
        private readonly IWebHostEnvironment _hostingEnvironment;

        public ProjectController(AppDbContext context, UserManager<Student> userManager, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _hostingEnvironment = hostingEnvironment;
        }

        // Show All Projects
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);


            var projects = _context.Projects
                .Where(p => p.Members.Any(m => m.StudentId == user.StudentId))
                .Include(p => p.Members)
                .ToList();
            return View(projects);
        }

        // Show my Projects
        public async Task<IActionResult> MyProjects()
        {

            var user = await _userManager.GetUserAsync(User);

            var projects = _context.Projects
                .Where(p => p.Members.Any(m => m.StudentId == user.StudentId))
                .Include(p => p.Members)
                .Take(3)
                .ToList();

            return View(projects);
        }

        // Show Project Details
        public IActionResult Details(int id)
        {
            var project = _context.Projects
                .Include(p => p.Members)
                .FirstOrDefault(p => p.ProjectId == id);
            return View(project);
        }


        // Create a new Project
        public async Task<IActionResult> CreateProject()
        {
            var students = _context.Students.Select(s => new StudentViewModel
            {
                StudentId = s.StudentId,
                FullName = $"{s.FirstName} {s.LastName}"
            }).ToList();

            var user = await _userManager.GetUserAsync(User);
            var mentors = _context.Mentors.ToList();


            var model = new ProjectViewModel
            {
                AvailableStudents = students,
                CreatorStudentId = user.Id,
                mentors = mentors
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProject(ProjectViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Create a new project from the ViewModel
                var project = new Project
                {
                    Title = model.Title,
                    Description = model.Description,
                    TeamSize = model.SelectedStudentIds.Count + 1,
                    StartDate = model.StartDate.ToUniversalTime(),
                    EndDate = model.EndDate.ToUniversalTime(),
                    Members = new List<Student>(),
                   MentorId = model.SelectedMentorId
                };

                // Retrieve the creator student from the database
                var creatorStudent = await _userManager.FindByIdAsync(model.CreatorStudentId);
                if (creatorStudent != null)
                {
                    project.Members.Add(creatorStudent);
                }
                else
                {
                    // Handle error: Creator student not found
                    ModelState.AddModelError("", "The project creator was not found.");
                    return View(model); // Assuming a view that takes this ViewModel
                }

                // Assign selected students to the project
                foreach (var studentId in model.SelectedStudentIds)
                {
                    var student = await _context.Users.FirstAsync(s => s.StudentId == studentId);
                    if (student != null)
                    {
                        project.Members.Add(student);
                    }
                    // Optionally handle the case where a student isn't found
                }

                

                // Save the project to the database
                _context.Add(project);
                await _context.SaveChangesAsync();

                if (!model.Files.IsNullOrEmpty())
                {
                    foreach (var file in model.Files)
                    {

                        // Save the file to the server
                        // For example, you could save it to the wwwroot/uploads folder
                        var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "uploads");
                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }

                        // Save the file path to the database
                        var projectFile = new RisingStarsData.Entities.Document
                        {
                            ProjectId = project.ProjectId,
                            FileName = uniqueFileName,
                        };
                        _context.Add(projectFile);
                    }
                }

                await _context.SaveChangesAsync();

                // Redirect to a confirmation page or the project details page
                return RedirectToAction("Details", new { id = project.ProjectId });
            }

            var students = _context.Students.Select(s => new StudentViewModel
            {
                StudentId = s.StudentId,
                FullName = $"{s.FirstName} {s.LastName}"
            }).ToList();

            var mentors = _context.Mentors.ToList();

            model.AvailableStudents = students;
            model.mentors = mentors;

            // If we got this far, something failed, redisplay form
            return View(model); // Assuming a view that takes this ViewModel
        }

        // Edit Project
        public IActionResult Edit(int id)
        {
            var project = _context.Projects
                .Include(p => p.Members)
                .FirstOrDefault(p => p.ProjectId == id);

            var students = _context.Students.Select(s => new StudentViewModel
            {
                StudentId = s.StudentId,
                FullName = $"{s.FirstName} {s.LastName}"
            }).ToList();

            var model = new ProjectViewModel
            {
                Title = project.Title,
                Description = project.Description,
                StartDate = project.StartDate.ToUniversalTime(),
                EndDate = project.EndDate.ToUniversalTime(),
                CreatorStudentId = project.Members.FirstOrDefault()?.Id,
                AvailableStudents = students,
                mentors = _context.Mentors.ToList(),
                SelectedMentorId = project.MentorId
            };

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(int id, ProjectViewModel model)
        {
            if (ModelState.IsValid)
            {
                var project = _context.Projects.Find(id);
                if (project == null)
                {
                    return NotFound();
                }

                project.Title = model.Title;
                project.Description = model.Description;
                project.TeamSize = model.SelectedStudentIds.Count + 1;
                project.StartDate = model.StartDate.ToUniversalTime();
                project.EndDate = model.EndDate.ToUniversalTime();
                project.MentorId = model.SelectedMentorId;

                // Retrieve the creator student from the database
                var creatorStudent = await _userManager.FindByIdAsync(model.CreatorStudentId);
                if (creatorStudent != null)
                {
                    project.Members.Clear();
                    project.Members.Add(creatorStudent);
                }
                else
                {
                    // Handle error: Creator student not found
                    ModelState.AddModelError("", "The project creator was not found.");
                    return View(model); // Assuming a view that takes this ViewModel
                }
                var memberIds = project.Members.Select(m => m.StudentId).ToList();
                foreach (var memberId in memberIds)
                {
                    if (!model.SelectedStudentIds.Contains(memberId))
                    {
                        var memberToRemove = project.Members.FirstOrDefault(m => m.StudentId == memberId);
                        if (memberToRemove != null)
                        {
                            project.Members.Remove(memberToRemove);
                        }
                    }
                }

                foreach (var studentId in model.SelectedStudentIds)
                {
                    if (!project.Members.Any(m => m.StudentId == studentId))
                    {
                        var studentToAdd = await _context.Users.FirstAsync(s => s.StudentId == studentId);
                        if (studentToAdd != null)
                        {
                            project.Members.Add(studentToAdd);
                        }
                    }
                }

                if (!model.Files.IsNullOrEmpty())
                {

                    // Delete existing files
                    var existingFiles = _context.Documents.Where(d => d.ProjectId == project.ProjectId).ToList();
                    foreach (var file in existingFiles)
                    {
                        _context.Remove(file);
                    }

                    // Save new files

                    foreach (var file in model.Files)
                    {

                        // Save the file to the server
                        // For example, you could save it to the wwwroot/uploads folder
                        var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "uploads");
                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }

                        // Save the file path to the database
                        var projectFile = new RisingStarsData.Entities.Document
                        {
                            ProjectId = project.ProjectId,
                            FileName = uniqueFileName,
                        };
                        _context.Add(projectFile);
                    }
                }

                // Now proceed with the update
                _context.Update(project);
                await _context.SaveChangesAsync();

                // Redirect to a confirmation page or the project details page
                return RedirectToAction("Details", new { id = project.ProjectId });
            }

            // If we got this far, something failed, redisplay form
            return View(model); // Assuming a view that takes this ViewModel
        }

        // Delete Project
        public IActionResult Delete(int id)
        {
            var project = _context.Projects.Find(id);
            return View(project);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var project = _context.Projects.Find(id);
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }




    }

}
