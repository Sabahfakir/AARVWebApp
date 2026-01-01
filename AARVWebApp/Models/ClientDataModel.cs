using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AARVWebApp.Models
{
    public class ClientDataModel
    {
        [Key]
        public int ClientID { get; set; }

        [Required, StringLength(150)]
        public string ClientName { get; set; }

        [StringLength(200)]
        public string ProjectName { get; set; }

        [StringLength(20)]
        public string PhoneNumber { get; set; }

        [StringLength(20)]
        public string WhatsAppNumber { get; set; }

        [EmailAddress, StringLength(100)]
        public string Email { get; set; }

        [StringLength(100)]
        public string Reference { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }

        [StringLength(250)]
        public string Address { get; set; }

        [StringLength(100)]
        public string City { get; set; }

        [StringLength(100)]
        public string State { get; set; }

        [StringLength(100)]
        public string Country { get; set; }

        public int? CreatedById { get; set; }
        public int? UpdatedById { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? CreatedDate { get; set; } = DateTime.Now;

        [DataType(DataType.DateTime)]
        public DateTime? UpdatedDate { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
    }
}