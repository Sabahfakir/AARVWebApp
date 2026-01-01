using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using AARVWebApp.Models;
using System.Net.Mail;


namespace AARVWebApp.Controllers
{
    public class HomeController : Controller
    {

        private AARVWebDBEntities db = new AARVWebDBEntities();
        public ActionResult Index()
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Home");

            DashboardViewModel model = new DashboardViewModel();

            // === Total Statistics ===
            model.TotalProjects = db.ProjectDetails.Count(p => p.IsDeleted != true);
            model.ActiveProjects = db.ProjectDetails.Count(p => p.IsActive == true && p.IsDeleted != true);
            model.CompletedProjects = db.ProjectDetails.Count(p => p.Project_Status.Status == "Completed" && p.IsDeleted != true);

            model.TotalClients = db.ClientMasters.Count(c => c.IsDeleted != true);
            model.TotalEmployees = db.EmployeeMasters.Count();
            model.TotalCategories = db.Project_Category.Count();

            // === Recent Projects ===
            model.RecentProjects = db.ProjectDetails
                .Where(p => p.IsDeleted != true)
                .OrderByDescending(p => p.CreatedDate)
                .Take(5)
                .ToList()
                .Select(p => new RecentProjectItem
                {
                    ProjectName = p.ProjectName,

                    // FIXED (no navigation property)
                    ClientName = db.ClientMasters
                        .Where(c => c.ClientID == p.ClientID)
                        .Select(c => c.ClientName)
                        .FirstOrDefault(),

                    ProjectManager = p.ProjectManager,
                    StartDate = p.StartDate,
                    DueDate = p.DueDate ?? DateTime.Now,
                    Status = p.Project_Status.Status,
                    StatusClass = GetStatusClass(p.Project_Status.Status)
                })
                .ToList();

            var monthlyData = db.ProjectDetails
        .Where(p => p.IsDeleted != true)
        .GroupBy(p => new { p.StartDate.Year, p.StartDate.Month })
        .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
        .Select(g => new
        {
            Month = g.Key.Month + "/" + g.Key.Year,
            Count = g.Count()
        })
        .ToList();

            model.Months = monthlyData.Select(m => m.Month).ToList();
            model.ProjectCount = monthlyData.Select(m => m.Count).ToList();


            // === Project Status Distribution ===
            var statusGroups = db.ProjectDetails
                .Where(p => p.IsDeleted != true)
                .GroupBy(p => p.Project_Status.Status)
                .Select(g => new
                {
                    StatusName = g.Key,
                    Count = g.Count()
                })
                .ToList();

            model.StatusLabels = statusGroups.Select(g => g.StatusName).ToList();
            model.ProjectStatusCount = statusGroups.Select(g => g.Count).ToList();

            return View(model);
        }

        // ======== Status Badge Color Mapping ========
        private string GetStatusClass(string status)
        {
            switch (status)
            {
                case "New":
                    return "badge bg-primary";
                case "In Progress":
                    return "badge bg-warning text-dark";
                case "Completed":
                    return "badge bg-success";
                case "On Hold":
                    return "badge bg-secondary";
                case "Cancelled":
                    return "badge bg-danger";
                default:
                    return "badge bg-dark";
            }
        }




        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string Username, string Password)
        {
            var user = db.UserMasters
                         .FirstOrDefault(u => u.Username == Username && u.Password == Password);

            if (user != null)
            {
                // Save Sessions
                Session["UserID"] = user.User_ID;
                Session["FullName"] = user.FirstName + " " + user.LastName;
                Session["Username"] = user.Username;
                Session["Role"] = user.Role;   // <-- NEW ROLE SESSION

                // Login Time
                string loginTime = DateTime.Now.ToString("dd MMM yyyy | hh:mm tt");

                // Save Login History
                SaveLoginHistory(user.User_ID);

                // Send Login Email
                var fullName = user.FirstName + " " + user.LastName;
                var html = LoginEmailTemplate(fullName, loginTime);
                SendHtmlEmail(user.Email, "Login Successful", html);

                return RedirectToAction("Index", "Home");
            }

            ViewBag.ErrorMessage = "Invalid Username or Password";
            return View();
        }


        // ---------------------- SAVE LOGIN HISTORY ----------------------
        private void SaveLoginHistory(int userId)
        {
            db.LoginHistories.Add(new LoginHistory
            {
                UserId = userId,
                LoginTime = DateTime.Now,
                IpAddress = Request.UserHostAddress,
                BrowserInfo = Request.Browser.Browser + " " + Request.Browser.Version
            });

            db.SaveChanges();
        }

        private string LoginEmailTemplate(string name, string loginTime)
        {
            return $@"
    <div style='font-family: Arial; padding:20px;'>
        <h2 style='color:#4A90E2;'>Login Successful</h2>
        <p>Hello <strong>{name}</strong>,</p>

        <p>You have successfully logged into your account.</p>
        
        <table style='margin-top:15px;'>
            <tr>
                <td><strong>Login Time:</strong></td>
                <td>{loginTime}</td>
            </tr>
        </table>

        <br />

        <p style='font-size:13px;color:#888;'>Regards,<br>AARV Web App Team</p>
    </div>";
        }

        private void SendHtmlEmail(string toEmail, string subject, string htmlBody)
        {
            try
            {
                var fromEmail = "aarvwebapp@gmail.com";
                var password = "hlhq edyt xdqc ltcw"; // Gmail App Password

                MailMessage mail = new MailMessage();
                mail.To.Add(toEmail);
                mail.From = new MailAddress(fromEmail);
                mail.Subject = subject;
                mail.Body = htmlBody;
                mail.IsBodyHtml = true;

                SmtpClient smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromEmail, password)
                };

                smtp.Send(mail);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Email Error: " + ex.Message);
            }
        }



        public ActionResult Logout()
        {
            // Clear all session variables
            Session.Clear();

            // Optionally abandon the session completely
            Session.Abandon();

            // Redirect user back to Login page
            return RedirectToAction("Login", "Home");
        }


        public ActionResult UpcomingDeadlines(int page = 1)
        {
            int pageSize = 10;
            DateTime today = DateTime.Now;
            DateTime next7 = today.AddDays(7);

            var data = (from p in db.ProjectDetails
                        join c in db.ClientMasters on p.ClientID equals c.ClientID into clientJoin
                        from c in clientJoin.DefaultIfEmpty()

                        join emp in db.EmployeeMasters on p.ProjectManagerID equals emp.Emp_ID into empJoin
                        from emp in empJoin.DefaultIfEmpty()

                        join st in db.Project_Status on p.StatusID equals st.Status_ID into statusJoin
                        from st in statusJoin.DefaultIfEmpty()

                        where p.DueDate != null
                              && p.IsDeleted == false
                              && p.DueDate >= today
                              && p.DueDate <= next7

                        select new UpcomingDeadlineVM
                        {
                            ProjectID = p.ProjectID,
                            ProjectName = p.ProjectName,

                            ClientName = c != null ? c.ClientName : "Unknown Client",

                            Status = st != null ? st.Status : "Unknown",

                            StartDate = p.StartDate,
                            DueDate = p.DueDate,

                            EmployeeName = emp != null
                                ? emp.FirstName + " " + emp.LastName
                                : "Not Assigned",

                            Priority = p.StatusID == 1 ? "High" :
                                       p.StatusID == 2 ? "Medium" :
                                       "Low",

                            DaysLeft = (int)DbFunctions.DiffDays(today, p.DueDate)
                        })
                        .OrderBy(x => x.DaysLeft)
                        .ToList();

            // Pagination
            int totalRecords = data.Count();
            var pagedData = data
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalRecords = totalRecords;

            return View(pagedData);
        }



        public ActionResult DailyUserActivityReport(DateTime? fromDate, DateTime? toDate, int page = 1)
        {
            int pageSize = 10;

            // Base query
            var query = db.LoginHistories.AsQueryable();

            // Date filter
            if (fromDate.HasValue)
                query = query.Where(x => DbFunctions.TruncateTime(x.LoginTime) >= DbFunctions.TruncateTime(fromDate));

            if (toDate.HasValue)
                query = query.Where(x => DbFunctions.TruncateTime(x.LoginTime) <= DbFunctions.TruncateTime(toDate));

            // Prepare result
            var result = query
                .OrderByDescending(x => x.LoginTime)
                .Select(x => new DailyUserActivityVM
                {
                    Username = x.UserMaster.FirstName + " " + x.UserMaster.LastName,
                    LoginTime = x.LoginTime,
                    IP = x.IpAddress,
                    Browser = x.BrowserInfo,
                    TotalLogins = x.UserMaster.LoginHistories.Count()
                })
                .ToList();

            int totalRecords = result.Count();
            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            // Pass data
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalRecords = totalRecords;
            ViewBag.TotalPages = totalPages;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

            return View(result);
        }



      






        public ActionResult AddUser()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddUser(UserMaster user)
        {
            if (ModelState.IsValid)
            {
                db.UserMasters.Add(user);
                db.SaveChanges();
                ViewBag.Message = "User added successfully!";
                return RedirectToAction("UserList"); // redirect after save
            }

            return View(user);
        }
        public ActionResult UserList()
        {
            var users = db.UserMasters.ToList();
            return View(users);
        }

        public ActionResult UserDetails(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var user = db.UserMasters.Find(id);
            if (user == null)
                return HttpNotFound();

            return View(user);
        }

        // 3️⃣ EDIT USER - GET
        public ActionResult UserEdit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var user = db.UserMasters.Find(id);
            if (user == null)
                return HttpNotFound();

            return View(user);
        }

        // 3️⃣ EDIT USER - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UserEdit(UserMaster user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.Entry(user).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("UserList");
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException ex)
                {
                    foreach (var eve in ex.EntityValidationErrors)
                    {
                        Console.WriteLine($"Entity \"{eve.Entry.Entity.GetType().Name}\" has validation errors:");
                        foreach (var ve in eve.ValidationErrors)
                        {
                            Console.WriteLine($"- Property: {ve.PropertyName}, Error: {ve.ErrorMessage}");
                        }
                    }

                    throw; // rethrow to see the full stack trace in Visual Studio
                }
            }
            return View(user);
        }

        // 4️⃣ DELETE USER - GET
        public ActionResult UserDelete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var user = db.UserMasters.Find(id);
            if (user == null)
                return HttpNotFound();

            return View(user);
        }

        // 4️⃣ DELETE USER - POST
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var user = db.UserMasters.Find(id);
            db.UserMasters.Remove(user);
            db.SaveChanges();
            return RedirectToAction("UserList");
        }



      


       



    }
}