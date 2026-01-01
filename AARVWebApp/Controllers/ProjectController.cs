using AARVWebApp.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AARVWebApp.Controllers
{
    public class ProjectController : Controller
    {


        private AARVWebDBEntities db = new AARVWebDBEntities();
        public ActionResult AddClient()
        {
            return View();
        }

        [HttpPost]
        public JsonResult SaveClient(ClientMaster model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    model.CreatedDate = DateTime.Now;
                    model.IsActive = true;
                    model.IsDeleted = false;

                    db.ClientMasters.Add(model);
                    db.SaveChanges();

                    return Json(new { success = true, message = "Client saved successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = "Validation failed. Please check your input." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }


        [HttpGet]
        public ActionResult AddProj()
        {
            ViewBag.CategoryList = new SelectList(db.Project_Category.ToList(), "Category_ID", "CategoryName");
            ViewBag.ClientList = new SelectList(db.ClientMasters.ToList(), "ClientID", "ClientName");
            ViewBag.StatusList = new SelectList(db.Project_Status.ToList(), "Status_ID", "Status");

            return View();
        }

        [HttpPost]
        public ActionResult AddProj(ProjectDetail model, IEnumerable<HttpPostedFileBase> projectFiles)
        {
            try
            {
                // 1️⃣ Create folder path (under your project root)
                string folderPath = Server.MapPath("~/UploadedFiles/Projects/");

                // 2️⃣ Check and create if missing
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // 3️⃣ Handle file uploads
                if (projectFiles != null)
                {
                    foreach (var file in projectFiles)
                    {
                        if (file != null && file.ContentLength > 0)
                        {
                            string fileName = Path.GetFileName(file.FileName);
                            string fullPath = Path.Combine(folderPath, fileName);
                            file.SaveAs(fullPath);
                        }
                    }
                }

                // 4️⃣ Save project details to database here
                model.CreatedDate = DateTime.Now;
                model.IsActive = true;
                db.ProjectDetails.Add(model);
                db.SaveChanges();

                TempData["Message"] = "✅ Project saved successfully!";
                return RedirectToAction("AddProj");
            }
            catch (Exception ex)
            {
                // Log the error if needed
                TempData["Error"] = "❌ Error: " + ex.Message;
                return RedirectToAction("AddProj");
            }
        }

        public ActionResult ProjectList()
        {
            // Include related tables like Status, Category, and Employee
            var projects = db.ProjectDetails
         .Include("Project_Status")
         .Include("Project_Category")
         .Include("EmployeeMaster")
         .ToList();

            return View(projects);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public ActionResult ViewProject(int id)
        {
            var project = db.ProjectDetails
         .Include(p => p.Project_Category)
         .Include(p => p.Project_Status)
         .AsNoTracking()
         .FirstOrDefault(p => p.ProjectID == id);

            if (project == null)
                return HttpNotFound();



            return View(project);
        }
        public ActionResult EditProject(int id)
        {
            var project = db.ProjectDetails.Find(id);
            if (project == null)
                return HttpNotFound();

            ViewBag.CategoryList = db.Project_Category.ToList();
            ViewBag.StatusList = db.Project_Status.ToList();
           
            return View(project);
        }

        [HttpPost]
        public ActionResult EditProject(ProjectDetail model)
        {
            ViewBag.CategoryList = db.Project_Category.ToList();
            ViewBag.StatusList = db.Project_Status.ToList();
        
            if (!ModelState.IsValid)
                return View(model);

            var project = db.ProjectDetails.Find(model.ProjectID);
            if (project == null)
                return HttpNotFound();

            project.ProjectName = model.ProjectName;
            project.CategoryID = model.CategoryID;   // FIXED
            project.StatusID = model.StatusID;       // FIXED
            project.ClientID = model.ClientID;
            project.Description = model.Description;
            project.StartDate = model.StartDate;
            project.DueDate = model.DueDate;
            project.UpdatedDate = DateTime.Now;

            db.SaveChanges();

            TempData["Success"] = "Project updated successfully!";
            return RedirectToAction("EditProject", new { id = model.ProjectID });
        }


        // ✅ Helper to bind dropdowns
        private void BindDropdowns()
        {
            ViewBag.CategoryList = new SelectList(db.Project_Category.ToList(), "Category_ID", "CategoryName");
            ViewBag.StatusList = new SelectList(db.Project_Status.ToList(), "Status_ID", "Status");
            ViewBag.ClientList = new SelectList(db.ClientMasters.ToList(), "ClientID", "ClientName");
        }








        // ✅ Delete Project (GET)
        [HttpGet]
        public ActionResult DeleteProject(int id)
        {
            var project = db.ProjectDetails
                .Include(p => p.Project_Category)
                .Include(p => p.Project_Status)
                .FirstOrDefault(p => p.ProjectID == id);

            if (project == null)
                return HttpNotFound();

            return View(project);
        }

        // ✅ Delete Project (POST)
        [HttpPost, ActionName("DeleteProject")]
        [ValidateAntiForgeryToken]
        public ActionResult ProjectDeleteConfirmed(int id)
        {
            var project = db.ProjectDetails.Find(id);
            if (project != null)
            {
                db.ProjectDetails.Remove(project);
                db.SaveChanges();
            }

            return RedirectToAction("ProjectList");
        }

        public ActionResult ClientList()
        {
            var clients = db.ClientMasters
                      .Where(c => c.IsDeleted == false)
                      .OrderByDescending(c => c.CreatedDate)
                      .ToList();
            return View(clients);
        }

        public ActionResult ViewClient(int id)
        {
            var client = db.ClientMasters.FirstOrDefault(c => c.ClientID == id);
            if (client == null)
                return HttpNotFound();

            return View(client);
        }

        // ✅ EDIT (GET)
        [HttpGet]
        public ActionResult EditClient(int id)
        {
            var client = db.ClientMasters.FirstOrDefault(c => c.ClientID == id);
            if (client == null)
                return HttpNotFound();

            return View(client);
        }

        // ✅ EDIT (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditClient(ClientMaster model)
        {
            if (ModelState.IsValid)
            {
                var existing = db.ClientMasters.Find(model.ClientID);
                if (existing != null)
                {
                    existing.ClientName = model.ClientName;
                    existing.ProjectName = model.ProjectName;
                    existing.PhoneNumber = model.PhoneNumber;
                    existing.WhatsAppNumber = model.WhatsAppNumber;
                    existing.Email = model.Email;
                    existing.Reference = model.Reference;
                    existing.Notes = model.Notes;
                    existing.Address = model.Address;
                    existing.City = model.City;
                    existing.State = model.State;
                    existing.Country = model.Country;
                    existing.UpdatedDate = DateTime.Now;

                    db.SaveChanges();
                    return RedirectToAction("ClientList");
                }
            }
            return View(model);
        }

        // ✅ DELETE CONFIRMATION PAGE
        [HttpGet]
        public ActionResult DeleteClient(int id)
        {
            var client = db.ClientMasters.FirstOrDefault(c => c.ClientID == id);
            if (client == null)
                return HttpNotFound();

            return View(client);
        }

        // ✅ DELETE (POST)
        [HttpPost, ActionName("DeleteClient")]
        [ValidateAntiForgeryToken]
        public ActionResult ClientDeleteConfirmed(int id)
        {
            var client = db.ClientMasters.Find(id);
            if (client != null)
            {
                client.IsDeleted = true;
                db.SaveChanges();
            }
            return RedirectToAction("ClientList");
        }

    }
}