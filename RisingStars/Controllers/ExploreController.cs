using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RisingStars.Models;
using RisingStarsData.DataAccess;
using RisingStarsData.Entities;

namespace RisingStars.Controllers
{
    public class ExploreController : Controller
    {

        private readonly AppDbContext _context;

        private readonly UserManager<Student> _userManager;

        private readonly IWebHostEnvironment _hostingEnvironment;

        public ExploreController(AppDbContext context, UserManager<Student> userManager, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _hostingEnvironment = hostingEnvironment;
        }

        public IActionResult Index()
        {
            var projects = _context
                .Projects
                .Include(p => p.Members)
                .ToList();

            return View(projects);
        }


        [HttpGet]
        [Route("Explore/{id}")]
        public IActionResult Details(int id)
        {
            var project = _context.Projects
                .Include(p => p.Members)
                .Include(p => p.Mentor)
                .Include(p => p.Documents)
                .Include(p => p.Comments)
                .ThenInclude(c => c.Student)
                .FirstOrDefault(p => p.ProjectId == id);

            ViewBag.Route = _hostingEnvironment.WebRootPath + "/uploads/";

            if (project == null)
            {
                return NotFound();
            }

            var viewModel = new ExploreViewModel()
            {
                Project = project,
                Id = project.ProjectId,
            };

            return View(viewModel);
        }

        [HttpPost]
        [Route("Explore/{id}")]
        public IActionResult AddComment(ExploreViewModel viewModel)
        {
            var project = _context.Projects
                .Include(p => p.Members)
                .FirstOrDefault(p => p.ProjectId == viewModel.Id);

            if (project == null)
            {
                return NotFound();
            }

            var studentId = _userManager.GetUserId(User);

            if (studentId == null)
            {
                   return RedirectToAction("Login", "Account");
            }

            project.Comments.Add(new Comment()
            {
                ProjectId = project.ProjectId,
                StudentId = studentId,
                Content = viewModel.Comment
            });

            _context.SaveChanges();

            return RedirectToAction("Details", new { id = project.ProjectId });
        }

        //                <a href="@Url.Action("Download", "Explore", new { id = document.DocumentId })">@document.FileName</a>

        [HttpGet]
        [Route("Explore/Download/{id}")]
        public IActionResult Download(int id)
        {
            var document = _context.Documents.FirstOrDefault(d => d.DocumentId == id);

            if (document == null)
            {
                return NotFound();
            }

            var path = _hostingEnvironment.WebRootPath + "/uploads/" + document.FileName;

            var memory = new MemoryStream();

            using (var stream = new FileStream(path, FileMode.Open))
            {
                stream.CopyTo(memory);
            }

            memory.Position = 0;

            return File(memory, GetContentType(path), Path.GetFileName(path));
        }

        private string GetContentType(string path)
        {
            var types = GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types[ext];
        }

        private Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
            {
                {".txt", "text/plain"},
                {".pdf", "application/pdf"},
                {".doc", "application/vnd.ms-word"},
                {".docx", "application/vnd.ms-word"},
                {".xls", "application/vnd.ms-excel"},
                {".xlsx", "application/vnd.ms-excel"},
                {".png", "image/png"},
                {".jpg", "image/jpeg"},
                {".jpeg", "image/jpeg"},
                {".gif", "image/gif"},
                {".mp4", "video/mp4"},
                {".csv", "text/csv"}
            };
        }

    }
}
