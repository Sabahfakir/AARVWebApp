using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AARVWebApp.Models
{
    public class ProjectCategoryModel
    {
        
            [Key]
            public int CategoryID { get; set; }

            [Required, StringLength(100)]
            public string CategoryName { get; set; }
        }

    }
