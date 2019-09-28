using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebApplication.Models;

namespace WebApplication.Controllers
{
    public class AccountController : Controller
    {
        public ActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Files");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel user)
        {
            if (ModelState.IsValid)
            {
                UserContext db = new UserContext();

                User acc = db.Users.FirstOrDefault(u => u.UserName == user.UserName && u.Password == user.Password);

                if (acc != null)
                {
                    FormsAuthentication.SetAuthCookie(user.UserName, true);

                    return RedirectToAction("Files");
                }

                ModelState.AddModelError("", "Wrong login or password");
            }
            return View();
        }

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterModel user)
        {
            if (ModelState.IsValid)
            {
                UserContext db = new UserContext();

                User acc = db.Users.FirstOrDefault(u => u.UserName == user.UserName);

                if (acc == null)
                {
                    db.Users.Add(new User { UserName = user.UserName, Password = user.Password });
                    db.SaveChanges();

                    FormsAuthentication.SetAuthCookie(user.UserName, true);

                    return RedirectToAction("Files");
                }

                ModelState.AddModelError("", "User already exists");
            }
            return View();
        }

        public ActionResult Files()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login");
            }

            UserContext db = new UserContext();

            User user = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);

            var files = db.Files.Where(f => f.UserId == user.Id);

            return View(files);
        }

        public ActionResult UploadFile()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login");
            }

            if (Request.Files.Count > 0)
            {
                UserContext db = new UserContext();

                for (int i = 0; i < Request.Files.Count; i++)
                {
                    var file = Request.Files[i];

                    if (file != null && file.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(file.FileName);
                        var path = Server.MapPath("~/UsersFiles/" + User.Identity.Name + "/");
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }
                        var filePath = Path.Combine(path, fileName);
                        if (!System.IO.File.Exists(filePath))
                        {
                            file.SaveAs(filePath);

                            int userId = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name).Id;

                            db.Files.Add(new Models.File { FileName = fileName, FileSize = file.ContentLength, UploadingDate = DateTime.Now, UserId = userId });
                        }
                    }
                }
                db.SaveChanges();
            }
            return RedirectToAction("Files");
        }

        public ActionResult DeleteFile(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login");
            }

            var db = new UserContext();

            var file = db.Files.FirstOrDefault(f => f.FileId == id);

            var path = Server.MapPath("~/UsersFiles/" + User.Identity.Name + "/" + file.FileName);

            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }

            db.Files.Remove(file);
            db.SaveChanges();

            return RedirectToAction("Files");
        }

        public ActionResult DownloadFile(string fileName)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login");
            }

            var path = Server.MapPath("~/UsersFiles/" + User.Identity.Name + "/" + fileName);

            if (System.IO.File.Exists(path))
            {
                byte[] fileBytes = System.IO.File.ReadAllBytes(path);

                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
            }

            return RedirectToAction("Files");
        }

        public ActionResult ExportToExcel()
        {
            var db = new UserContext();

            var userId = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name).Id;

            var files = db.Files.Where(f => f.UserId == userId).Select(a => new { a.FileName, a.FileSize, a.UploadingDate }).ToList();

            GridView gridview = new GridView();
            gridview.DataSource = files;
            gridview.DataBind();

            // Clear all the content from the current response
            Response.ClearContent();
            Response.Buffer = true;
            // set the header
            Response.AddHeader("content-disposition", "attachment; filename = files.xls");
            Response.ContentType = "application/ms-excel";
            Response.Charset = "";
            // create HtmlTextWriter object with StringWriter
            using (StringWriter sw = new StringWriter())
            {
                using (HtmlTextWriter htw = new HtmlTextWriter(sw))
                {
                    // render the GridView to the HtmlTextWriter
                    gridview.RenderControl(htw);
                    // Output the GridView content saved into StringWriter
                    Response.Output.Write(sw.ToString());
                    Response.Flush();
                    Response.End();
                }
            }

            return RedirectToAction("Files");
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }
    }
}