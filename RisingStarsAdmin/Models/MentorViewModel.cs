using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace RisingStarsAdmin.Models
{

    public class MentorViewModel
    {
        public int MentorId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Phone Number")]
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }
        [Required]
        public string Bio { get; set; }


        [Display(Name = "Profile Picture")]
        [RegularExpression(@"(http(s?):)([/|.|\w|\s|-])*\.(?:jpg|gif|png)", ErrorMessage = "Invalid URL. Please enter a valid URL and make sure it ends with .jpg, .gif, or .png")]
        public string ProfilePicture { get; set; }

    }
}
