using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebApplication.Models;

namespace WebApplication.Controllers
{
    public class FileController : Controller
    {
        [Authorize]
        public ActionResult Files()
        {            
            UserContext db = new UserContext();

            User user = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);

            var files = db.Files.Where(f => f.UserId == user.Id);

            ViewBag.Date = DateTime.Today.ToString("yyyy-MM-dd");

            return View(files);
        }

        [Authorize]
        [HttpPost]
        public ActionResult UploadFile()
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

        [Authorize]
        [HttpPost]
        public ActionResult DeleteFile(int id)
        {
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

        [Authorize]
        public ActionResult DownloadFile(string fileName)
        {
            var path = Server.MapPath("~/UsersFiles/" + User.Identity.Name + "/" + fileName);

            if (System.IO.File.Exists(path))
            {
                byte[] fileBytes = System.IO.File.ReadAllBytes(path);

                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
            }

            return RedirectToAction("Files");
        }

        [Authorize]
        public ActionResult ExportToExcel(DateRange date)
        {
            Type type = typeof(Models.File);

            var properties = type.GetProperties().Where(prop => !prop.IsDefined(typeof(Hidden), false));

            var model = new { properties };

            if (date.FirstDate > date.SecondDate)
            {
                var tmp = date.FirstDate;
                date.FirstDate = date.SecondDate;
                date.SecondDate = tmp;
            }

            date.SecondDate = date.SecondDate.AddDays(1);

            var db = new UserContext();

            var userId = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name).Id;

            var files = db.Files.Where(f => f.UserId == userId && f.UploadingDate >= date.FirstDate && f.UploadingDate <= date.SecondDate);
                //.Select(a => new { a.GetType().GetProperties().Where(prop => !prop.IsDefined(typeof(Hidden), false)) }).ToList();
            //var files = db.Files.Where(f => f.UserId == userId && f.UploadingDate >= date.FirstDate && f.UploadingDate <= date.SecondDate).Select(a => new { a.FileName, a.FileSize, a.UploadingDate }).ToList();

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

    }
}