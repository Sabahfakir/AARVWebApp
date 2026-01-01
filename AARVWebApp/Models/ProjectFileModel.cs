using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AARVWebApp.Models
{
    public class ProjectFileModel
    {
        [Key]
        public int FileID { get; set; }

        [Required]
        public int ProjectID { get; set; }

        [StringLength(300)]
        public string FilePath { get; set; }

        [ForeignKey("ProjectID")]
        public virtual ProjectDetail ProjectDetail { get; set; }
    }
}