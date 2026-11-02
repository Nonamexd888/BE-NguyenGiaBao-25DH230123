using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BE_NguyenGiaBao_25DH230123.Models.ViewModel
{
    public class CustomerSearchVM
    {
        public string SearchTerm { get; set; }
        public string SortOrder { get; set; }
        public IPagedList<Customer> Customers { get; set; }
    }
}