using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using RisingStarsData.Entities;
using System.ComponentModel.DataAnnotations;

namespace RisingStars.Models
{
    public class ProjectViewModel
    {
        // Project Details
        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }
        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime EndDate { get; set; } = DateTime.Now.AddMonths(1);

        // List of Students to choose from for assigning to the project
        public List<StudentViewModel> AvailableStudents { get; set; } = new List<StudentViewModel>();

        // IDs of the students selected to be assigned to the project
        [Required]
        public List<string> SelectedStudentIds { get; set; } = new List<string>();

        [ValidateNever]
        public string CreatorStudentId { get; set; }

        [ValidateNever]
        public List<Mentor> mentors { get; set; } = new List<Mentor>();

        [Required]
        public int SelectedMentorId { get; set; }


        public List<IFormFile> Files { get; set; } = new List<IFormFile>();


    }

}
