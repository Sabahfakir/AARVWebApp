using AARVWebApp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AARVWebApp.Controllers
{
    public class EmployeeController : Controller
    {
        // GET: Employee

        private AARVWebDBEntities db = new AARVWebDBEntities();
        [HttpGet]
        public ActionResult AddEmp(int? id)
        {
            var model = id.HasValue
                ? db.EmployeeMasters.FirstOrDefault(e => e.Emp_ID == id.Value) ?? new EmployeeMaster()
                : new EmployeeMaster();
            return View(model);
        }

        // ✅ Save each tab without full model validation
        [HttpPost]
        public JsonResult SavePartialTabData(string tabName, EmployeeMaster model)
        {
            try
            {
                EmployeeMaster emp;

                if (model.Emp_ID > 0)
                {
                    emp = db.EmployeeMasters.FirstOrDefault(e => e.Emp_ID == model.Emp_ID);
                    if (emp == null)
                        return Json(new { success = false, message = "Employee not found" });
                }
                else
                {
                    emp = new EmployeeMaster();
                    db.EmployeeMasters.Add(emp);
                }

                // ✅ Map fields only for the current tab
                switch (tabName)
                {
                    case "info":
                        emp.Emp_Code = model.Emp_Code;
                        emp.FirstName = model.FirstName;
                        emp.MiddleName = model.MiddleName;
                        emp.LastName = model.LastName;
                        emp.PersonalEmail = model.PersonalEmail;
                        emp.PhoneNo = model.PhoneNo;
                        emp.OfficeEmail = model.OfficeEmail;
                        emp.EmergCntName = model.EmergCntName;
                        emp.EmerPhoneNo = model.EmerPhoneNo;
                        emp.Gender = model.Gender;
                        emp.Department = model.Department;
                        emp.Designation = model.Designation;
                        emp.EmployeeType = model.EmployeeType;
                        emp.IsActive = model.IsActive;

                        // Handle file upload
                        if (Request.Files.Count > 0)
                        {
                            var file = Request.Files["ProfilePhoto"];
                            if (file != null && file.ContentLength > 0)
                            {
                                string uploadDir = Server.MapPath("~/Uploads/EmployeePhotos/");
                                if (!Directory.Exists(uploadDir))
                                    Directory.CreateDirectory(uploadDir);

                                string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                                string fullPath = Path.Combine(uploadDir, fileName);
                                file.SaveAs(fullPath);
                                emp.ProfilePhoto = "/Uploads/EmployeePhotos/" + fileName;
                            }
                        }
                        break;

                    case "employment":
                        emp.JoiningDate = model.JoiningDate;
                        emp.ReleaseDate = model.ReleaseDate;
                        emp.LastDate = model.LastDate;
                        break;


                    case "address":
                        // 🔹 New case added to save address
                        emp.P_Country = model.P_Country;
                        emp.P_State = model.P_State;
                        emp.P_City = model.P_City;
                        emp.P_Addresstype = model.P_Addresstype;
                        emp.P_Add1 = model.P_Add1;
                        emp.P_Add2 = model.P_Add2;

                        emp.C_Country = model.C_Country;
                        emp.C_State = model.C_State;
                        emp.C_City = model.C_City;
                        emp.C_Pincode = model.C_Pincode;
                        emp.C_Add1 = model.C_Add1;
                        emp.C_Add2 = model.C_Add2;
                        break;

                    case "other":
                        emp.AadharNo = model.AadharNo;
                        emp.PanNo = model.PanNo;
                        emp.EPFUANNo = model.EPFUANNo;
                        emp.WorkStatus = model.WorkStatus;
                        emp.WorkExperinence = model.WorkExperinence;
                        emp.EducationDetails = model.EducationDetails;
                        break;
                }

                db.Configuration.ValidateOnSaveEnabled = false;
                db.SaveChanges();
                db.Configuration.ValidateOnSaveEnabled = true;

                return Json(new
                {
                    success = true,
                    message = $"{tabName} tab saved successfully!",
                    empId = emp.Emp_ID
                });
            }
            catch (Exception ex)
            {
                var errorMessage = ex.Message;

                if (ex.InnerException != null)
                    errorMessage += " | Inner: " + ex.InnerException.Message;

                if (ex.InnerException?.InnerException != null)
                    errorMessage += " | Inner Inner: " + ex.InnerException.InnerException.Message;

                System.Diagnostics.Debug.WriteLine("💥 Error Details: " + errorMessage);

                return Json(new { success = false, message = errorMessage });
            }
        }

        // ✅ Final save when clicking “Save & Close”
        [HttpPost]
        public JsonResult SaveFinal(int empId)
        {
            try
            {
                var emp = db.EmployeeMasters.FirstOrDefault(e => e.Emp_ID == empId);
                if (emp == null)
                    return Json(new { success = false, message = "Employee not found!" });

                db.SaveChanges();
                return Json(new { success = true, message = "Employee saved successfully!" });
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                var errorMessage = ex.Message;

                if (ex.InnerException != null)
                    errorMessage += " | Inner: " + ex.InnerException.Message;

                if (ex.InnerException?.InnerException != null)
                    errorMessage += " | Inner Inner: " + ex.InnerException.InnerException.Message;

                System.Diagnostics.Debug.WriteLine("💥 Error Details: " + errorMessage);

                return Json(new { success = false, message = errorMessage });
            }


        }
        public ActionResult EmpList(string department, string status)
        {
            var employees = db.EmployeeMasters.AsQueryable();

            // Filtering
            if (!string.IsNullOrEmpty(department) && department != "Department")
            {
                employees = employees.Where(e => e.Department == department);
            }
            if (!string.IsNullOrEmpty(status) && status != "Employee Status")
            {
                employees = employees.Where(e => e.WorkStatus == status);
            }

            ViewBag.Department = department;
            ViewBag.Status = status;

            return View(employees.ToList());
        }

        public ActionResult EmpDetails(int id)
        {
            var emp = db.EmployeeMasters.FirstOrDefault(e => e.Emp_ID == id);
            if (emp == null)
                return HttpNotFound();
            return View(emp);
        }

        // ✅ EDIT PAGE (GET)
        public ActionResult EmpEdit(int id)
        {
            var emp = db.EmployeeMasters.FirstOrDefault(e => e.Emp_ID == id);
            if (emp == null)
                return HttpNotFound();
            return View(emp);
        }

        // ✅ EDIT PAGE (POST)
        [HttpPost]
        public ActionResult EmpEdit(EmpDataModel model)
        {
            if (ModelState.IsValid)
            {
                var existingEmp = db.EmployeeMasters.Find(model.Emp_ID);
                if (existingEmp == null)
                {
                    return HttpNotFound(); // No record found
                }

                // Update only the needed fields
                existingEmp.Emp_Code = model.Emp_Code;
                existingEmp.FirstName = model.FirstName;
                existingEmp.MiddleName = model.MiddleName;
                existingEmp.LastName = model.LastName;
                existingEmp.PersonalEmail = model.PersonalEmail;
                existingEmp.PhoneNo = model.PhoneNo;
                existingEmp.OfficeEmail = model.OfficeEmail;
                existingEmp.EmergCntName = model.EmergCntName;
                existingEmp.EmerPhoneNo = model.EmerPhoneNo;
                existingEmp.Gender = model.Gender;
                existingEmp.ProfilePhoto = model.ProfilePhoto;
                existingEmp.Department = model.Department;
                existingEmp.Designation = model.Designation;
                existingEmp.EmployeeType = model.EmployeeType;

                // Primary Address
                existingEmp.P_Country = model.P_Country;
                existingEmp.P_State = model.P_State;
                existingEmp.P_City = model.P_City;
                existingEmp.P_Addresstype = model.P_Addresstype;
                existingEmp.P_Add1 = model.P_Add1;
                existingEmp.P_Add2 = model.P_Add2;

                // Current Address
                existingEmp.C_Add1 = model.C_Add1;
                existingEmp.C_Add2 = model.C_Add2;
                existingEmp.C_Country = model.C_Country;
                existingEmp.C_State = model.C_State;
                existingEmp.C_City = model.C_City;
                existingEmp.C_Pincode = model.C_Pincode;

                // Other fields
                existingEmp.IsActive = model.IsActive;
                existingEmp.JoiningDate = model.JoiningDate;
                existingEmp.ReleaseDate = model.ReleaseDate;
                existingEmp.LastDate = model.LastDate;
                existingEmp.AadharNo = model.AadharNo;
                existingEmp.PanNo = model.PanNo;
                existingEmp.EPFUANNo = model.EPFUANNo;
                existingEmp.WorkStatus = model.WorkStatus;
                existingEmp.WorkExperinence = model.WorkExperinence;
                existingEmp.EducationDetails = model.EducationDetails;

                db.SaveChanges();
                return RedirectToAction("EmpList");
            }

            return View(model);
        }


        // ✅ DELETE CONFIRMATION PAGE
        public ActionResult EmpDelete(int id)
        {
            var emp = db.EmployeeMasters.FirstOrDefault(e => e.Emp_ID == id);
            if (emp == null)
                return HttpNotFound();
            return View(emp);
        }

        // ✅ DELETE CONFIRMED (POST)
        [HttpPost, ActionName("EmpDeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public ActionResult EmpDeleteConfirmed(int id)
        {
            var emp = db.EmployeeMasters.FirstOrDefault(e => e.Emp_ID == id);
            if (emp != null)
            {
                db.EmployeeMasters.Remove(emp);
                db.SaveChanges();
            }
            return RedirectToAction("EmpList");
        }
    }
}