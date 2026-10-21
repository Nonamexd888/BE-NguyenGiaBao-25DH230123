using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BE_NguyenGiaBao_25DH230123.Models.ViewModel
{
    public class ProductSearchVM
    {
        public string SearchTerm { get; set; }
        public List<Product> Products { get; set; }
    }
}