using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using WebApplication.Helpers;
using WebApplication.Models;

namespace WebApplication.Controllers
{
    public class AccountController : Controller
    {
        public ActionResult Login()
        {
            //if (User.Identity.IsAuthenticated)
            //{
            //    return RedirectToAction("Files", "Files");
            //}
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel user)
        {
            if (ModelState.IsValid)
            {
                UserContext db = new UserContext();

                var password = Hasher.Hash(user.Password);
                User acc = db.Users.FirstOrDefault(u => u.UserName == user.UserName && u.Password == password);

                if (acc != null)
                {
                    FormsAuthentication.SetAuthCookie(user.UserName, true);

                    return RedirectToAction("Files", "File");
                }

                ModelState.AddModelError("", "Wrong login or password");
            }
            return View();
        }

        public ActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Files", "File");
            }
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
                    var password = Hasher.Hash(user.Password);
                    db.Users.Add(new User { UserName = user.UserName, Password = password });
                    db.SaveChanges();

                    FormsAuthentication.SetAuthCookie(user.UserName, true);

                    return RedirectToAction("Files", "File");
                }

                ModelState.AddModelError("", "User already exists");
            }
            return View();
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }
    }
}