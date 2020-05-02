using System.Collections.Generic;
using SparkAuto.Data;

namespace SparkAuto.Models.ViewModel
{
    public class UserListViewModel
    {
        public List<ApplicationUser> ApplicationUserList { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}
