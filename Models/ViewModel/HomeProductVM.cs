using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList.Mvc;

namespace BE_NguyenGiaBao_25DH230123.Models.ViewModel
{
    public class HomeProductVM
    {
        // Tiêu chí tìm kiếm
        public string SearchTerm { get; set; }

        // Các thuộc tính hỗ trợ paging
        public int PageNumber { get; set; } // Vị trí trang hiện tại
        public int PageSize { get; set; } = 10;// Mổi trang có bao nhiêu bản ghi

        // Danh sách sản phẩm nồi bật
        public List<Product> FeaturedProducts { get; set; }

        // Danh sách sản phẩm mới đã phân trang
        public PagedList.IPagedList<Product> NewProducts { get; set; }
    }
}