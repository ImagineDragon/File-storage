using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace WebApplication.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Username")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }

    public class File
    {
        [Key]
        public int FileId { get; set; }

        [Required]
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

    public class LoginModel
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }

    public class RegisterModel
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Wrong confirm password")]
        public string ConfirmPassword { get; set; }
    }
}