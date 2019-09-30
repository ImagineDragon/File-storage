using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication.Models
{
    public class Hidden : Attribute { }

    public class File
    {
        [Key]
        [Hidden]
        public int FileId { get; set; }

        [Required]
        [Hidden]
        public int UserId { get; set; }

        [Required]
        [Display(Name = "File name")]
        public string FileName { get; set; }

        [Required]
        [Display(Name = "File size")]
        public int FileSize { get; set; }

        [Required]
        [Display(Name = "Uploading date")]
        public DateTime UploadingDate { get; set; }
    }

    public class DateRange
    {
        public DateTime FirstDate { get; set; }

        public DateTime SecondDate { get; set; }
    }
}