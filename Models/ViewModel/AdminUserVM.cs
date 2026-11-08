using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList;
using System.ComponentModel.DataAnnotations;

namespace BE_NguyenGiaBao_25DH230123.Models.ViewModel
{
    public class AdminUserVM
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [StringLength(255)]
        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; }

        [DataType(DataType.Password)]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        [Display(Name = "Xác nhận mật khẩu")]
        public string ConfirmPassword { get; set; }
    }

    public class AdminUserSearchVM
    {
        public string SearchTerm { get; set; }
        public IPagedList<User> Users { get; set; }
    }
}