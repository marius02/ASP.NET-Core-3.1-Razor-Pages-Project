using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SparkAuto.Data;
using SparkAuto.Models.ViewModel;
using SparkAuto.Utility;

namespace SparkAuto.Pages.Users
{
    [Authorize(Roles = SD.AdminEndUser)]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public IndexModel(ApplicationDbContext db)
        {
            _db = db;
        }

        [BindProperty]
        public  UserListViewModel UserListVm { get; set; }

        public async Task<IActionResult> OnGet(int productPage = 1, string searchEmail=null, string searchName=null, string searchPhone=null)
        {
            UserListVm = new UserListViewModel
            {
                ApplicationUserList = await _db.ApplicationUsers.ToListAsync()
            };
            
            var param = new StringBuilder();
            param.Append("/Users?productPage=:");

            param.Append("&searchName=");
            if (searchName != null)
            {
                param.Append(searchName);
            }

            param.Append("&searchEmail=");
            if (searchName != null)
            {
                param.Append(searchEmail);
            }

            param.Append("&searchPhone=");
            if (searchName != null)
            {
                param.Append(searchPhone);
            }

            if (searchEmail != null)
            {
                UserListVm.ApplicationUserList = await _db.ApplicationUsers
                    .Where(u => u.Email.ToLower().Contains(searchEmail.ToLower())).ToListAsync();
            }
            else
            {
                if (searchName != null)
                {
                    UserListVm.ApplicationUserList = await _db.ApplicationUsers
                        .Where(u => u.Name.ToLower().Contains(searchName.ToLower())).ToListAsync();
                }
                else
                {
                    if (searchPhone != null)
                    {
                        UserListVm.ApplicationUserList = await _db.ApplicationUsers
                            .Where(u => u.PhoneNumber.ToLower().Contains(searchPhone.ToLower())).ToListAsync();
                    }
                }
            }
            var count = UserListVm.ApplicationUserList.Count;

            UserListVm.PagingInfo = new PagingInfo
            {
                CurrentPage = productPage,
                ItemsPerPage = SD.PaginationUsersPage,
                TotalItems = count,
                UrlParam = param.ToString()
            };

            UserListVm.ApplicationUserList = UserListVm.ApplicationUserList.OrderBy(p => p.Email)
                .Skip((productPage - 1) * SD.PaginationUsersPage)
                .Take(SD.PaginationUsersPage).ToList();

            return Page();
        }
    }
}
