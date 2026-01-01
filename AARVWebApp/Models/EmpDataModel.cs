using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AARVWebApp.Models
{
    public class EmpDataModel
    {
        [Key]
        public int Emp_ID { get; set; }
        public string Emp_Code { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string PersonalEmail { get; set; }
        public string PhoneNo { get; set; }
        public string OfficeEmail { get; set; }
        public string EmergCntName { get; set; }
        public string EmerPhoneNo { get; set; }
        public string Gender { get; set; }
        public string ProfilePhoto { get; set; }
        public string Department { get; set; }
        public string Designation { get; set; }
        public string EmployeeType { get; set; }

        // Primary Address
        public string P_Country { get; set; }
        public string P_State { get; set; }
        public string P_City { get; set; }
        public string P_Addresstype { get; set; }
        public string P_Add1 { get; set; }
        public string P_Add2 { get; set; }

        // Current Address
        public string C_Add1 { get; set; }
        public string C_Add2 { get; set; }
        public string C_Country { get; set; }
        public string C_State { get; set; }
        public string C_City { get; set; }
        public string C_Pincode { get; set; }

        public bool IsActive { get; set; }

        // Employment Dates
        public DateTime? JoiningDate { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public DateTime? LastDate { get; set; }

        // KYC
        public string AadharNo { get; set; }
        public string PanNo { get; set; }
        public string EPFUANNo { get; set; }

        // Work Experience
        
        public string WorkStatus { get; set; } // Fresher / Experience
        public string WorkExperinence { get; set; }

        // Education
        public string EducationDetails { get; set; }
    }

}