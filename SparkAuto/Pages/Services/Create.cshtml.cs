using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SparkAuto.Data;
using SparkAuto.Models;
using SparkAuto.Models.ViewModel;
using SparkAuto.Utility;

namespace SparkAuto.Pages.Services
{
    [Authorize(Roles = SD.AdminEndUser)]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        [BindProperty]
        public CarServiceViewModel CarServiceViewModel { get; set; }

        public CreateModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> OnGet(int carId)
        {
            CarServiceViewModel = new CarServiceViewModel
            {
                Car = await _db.Cars.Include(c => c.ApplicationUser).FirstOrDefaultAsync(c => c.Id == carId),
                ServiceHeader = new Models.ServiceHeader()
            };

            List<String> lstServiceTypeInShoppingCart = _db.ServiceShoppingCarts
                                                            .Include(c => c.ServiceType)
                                                            .Where(c => c.CarId == carId)
                                                            .Select(c => c.ServiceType.Name)
                                                            .ToList();

            IQueryable<ServiceType> lstService = from s in _db.ServiceTypes
                                                 where !(lstServiceTypeInShoppingCart.Contains(s.Name))
                                                 select s;

            CarServiceViewModel.ServiceTypes = lstService.ToList();

            CarServiceViewModel.ServiceShoppingCarts = _db.ServiceShoppingCarts.Include(c => c.ServiceType).Where(c => c.CarId == carId).ToList();
            CarServiceViewModel.ServiceHeader.TotalPrice = 0;

            foreach (var item in CarServiceViewModel.ServiceShoppingCarts)
            {
                CarServiceViewModel.ServiceHeader.TotalPrice += item.ServiceType.Price;
            }

            return Page();

        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                CarServiceViewModel.ServiceHeader.DateAdded = DateTime.Now;
                CarServiceViewModel.ServiceShoppingCarts = _db.ServiceShoppingCarts.Include(c => c.ServiceType).Where(c => c.CarId == CarServiceViewModel.Car.Id).ToList();
                foreach (var item in CarServiceViewModel.ServiceShoppingCarts)
                {
                    CarServiceViewModel.ServiceHeader.TotalPrice += item.ServiceType.Price;
                }
                CarServiceViewModel.ServiceHeader.CarId = CarServiceViewModel.Car.Id;

                _db.ServiceHeaders.Add(CarServiceViewModel.ServiceHeader);
                await _db.SaveChangesAsync();

                foreach (var detail in CarServiceViewModel.ServiceShoppingCarts)
                {
                    ServiceDetails serviceDetails = new ServiceDetails
                    {
                        ServiceHeaderId = CarServiceViewModel.ServiceHeader.Id,
                        ServiceName = detail.ServiceType.Name,
                        ServicePrice = detail.ServiceType.Price,
                        ServiceTypeId = detail.ServiceTypeId
                    };

                    _db.ServiceDetails.Add(serviceDetails);

                }
                _db.ServiceShoppingCarts.RemoveRange(CarServiceViewModel.ServiceShoppingCarts);

                await _db.SaveChangesAsync();

                return RedirectToPage("../Cars/Index", new { userId = CarServiceViewModel.Car.UserId });
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAddToCart()
        {
            ServiceShoppingCart objServiceCart = new ServiceShoppingCart()
            {
                CarId = CarServiceViewModel.Car.Id,
                ServiceTypeId = CarServiceViewModel.ServiceDetails.ServiceTypeId
            };

            _db.ServiceShoppingCarts.Add(objServiceCart);
            await _db.SaveChangesAsync(); 
            return RedirectToPage("Create", new { carId = CarServiceViewModel.Car.Id });
        }

        public async Task<IActionResult> RemoveFromCart(int serviceTypeId)
        {
            ServiceShoppingCart objServiceCart = _db.ServiceShoppingCarts
                .FirstOrDefault(u => u.CarId == CarServiceViewModel.Car.Id && u.ServiceTypeId == serviceTypeId);

            _db.ServiceShoppingCarts.Remove(objServiceCart);
            await _db.SaveChangesAsync();
            return RedirectToPage("Create", new { carId = CarServiceViewModel.Car.Id });
        }
    }
}