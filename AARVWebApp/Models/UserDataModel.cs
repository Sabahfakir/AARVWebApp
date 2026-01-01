using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AARVWebApp.Models
{
    public class UserDataModel
    {
      
            [Key]
            public int User_ID { get; set; }

            [Required(ErrorMessage = "Username is required")]
            public string Username { get; set; }

            [Required(ErrorMessage = "Password is required")]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Required(ErrorMessage = "First name is required")]
            public string FirstName { get; set; }

            [Required(ErrorMessage = "Last name is required")]
            public string LastName { get; set; }

            [Required(ErrorMessage = "Email is required")]
            [EmailAddress]
            public string Email { get; set; }

            [Required(ErrorMessage = "Mobile number is required")]
            [Phone]
            public string Mobile { get; set; }

            [Required(ErrorMessage = "Role is required")]
            public string Role { get; set; }
        }
    }

