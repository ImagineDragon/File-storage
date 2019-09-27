using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
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

        public ActionResult FileUpload()
        {
            if (Request.Files.Count > 0)
            {
                UserContext db = new UserContext();

                for (int i = 0; i < Request.Files.Count; i++)
                {
                    var file = Request.Files[i];

                    if (file != null && file.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(file.FileName);
                        var path = Path.Combine(Server.MapPath("~/UsersFiles/"), fileName);
                        file.SaveAs(path);

                        int userId = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name).Id;

                        db.Files.Add(new Models.File { FileName = fileName, FileSize = file.ContentLength, UploadingDate = DateTime.Now, UserId = userId });
                    }
                }
                db.SaveChanges();
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