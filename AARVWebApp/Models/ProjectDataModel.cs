using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AARVWebApp.Models
{
    public class ProjectDataModel
    {
        [Key]
        public int ProjectID { get; set; }

        [Required, StringLength(200)]
        public string ProjectName { get; set; }

        [Required]
        public int CategoryID { get; set; }

        [Required]
        public int ClientID { get; set; }

        [Required, DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required, DataType(DataType.Date)]
        public DateTime DueDate { get; set; }

        [Required]
        public int StatusID { get; set; }

        public string Description { get; set; }

        [Required, StringLength(100)]
        public string ProjectManager { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("CategoryID")]
        public virtual Project_Category ProjectCategory { get; set; }

        [ForeignKey("ClientID")]
        public virtual ClientMaster ClientMaster { get; set; }

        [ForeignKey("StatusID")]
        public virtual Project_Status ProjectStatus { get; set; }

        public virtual ICollection<ProjectFile> ProjectFiles { get; set; }
    }

}
