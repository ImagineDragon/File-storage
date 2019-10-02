using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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

        public static object GetValue(object obj, List<PropertyInfo> propertyInfos)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            foreach (var p in propertyInfos)
            {
                var value = p.GetValue(obj);
                dictionary.Add(p.Name, value);
            }
            var expandoObject = new ExpandoObject();
            var keyValuePairs = (ICollection<KeyValuePair<string, object>>)expandoObject;
            foreach(var kvp in dictionary)
            {
                keyValuePairs.Add(kvp);
            }
            return expandoObject;
        }

        [Authorize]
        public ActionResult ExportToExcel(DateRange date)
        {
            Type type = typeof(Models.File);

            var properties = type.GetProperties().Where(prop => !prop.IsDefined(typeof(Hidden), false)).ToList();

            if (date.FirstDate > date.SecondDate)
            {
                var tmp = date.FirstDate;
                date.FirstDate = date.SecondDate;
                date.SecondDate = tmp;
            }

            date.SecondDate = date.SecondDate.AddDays(1);

            var db = new UserContext();

            var userId = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name).Id;

            var files = db.Files.Where(f => f.UserId == userId && f.UploadingDate >= date.FirstDate && f.UploadingDate <= date.SecondDate).ToList();

            try
            {
                DataTable Dt = new DataTable();
                foreach(var prop in properties)
                {
                    Dt.Columns.Add(prop.Name, prop.PropertyType);
                }

                foreach(var file in files)
                {
                    DataRow row = Dt.NewRow();
                    for(int i = 0; i < properties.Count; i++)
                    {
                        row[i] = properties[i].GetValue(file);
                    }
                    Dt.Rows.Add(row);
                }

                var memoryStream = new MemoryStream();
                using(var excelPackage = new ExcelPackage(memoryStream))
                {
                    var worksheet = excelPackage.Workbook.Worksheets.Add("Sheet1");
                    worksheet.Cells["A1"].LoadFromDataTable(Dt, true, TableStyles.None);

                    Session["DownloadExcel"] = excelPackage.GetAsByteArray();
                    return Json("");
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }

        [Authorize]
        public ActionResult DownloadExcel()
        {
            if (Session["DownloadExcel"] != null)
            {
                byte[] data = Session["DownloadExcel"] as byte[];
                return File(data, "application/octet-stream", "Files.xlsx");
            }
            else
            {
                return new EmptyResult();
            }
        }
    }
}