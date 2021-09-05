using System;
using System.Linq;
using System.Threading.Tasks;
using FoodPicker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodPicker.Controllers
{
    [Route("[controller]")]
    [Authorize(Policy = "AccessInternalAdminAreas")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            return View("List", users);
        }
        
        [HttpGet]
        [Route("Create")]
        [Route("{id}")]
        public async Task<IActionResult> CreateOrEdit(string id)
        {
            ApplicationUser model;
            if (string.IsNullOrEmpty(id))
            {
                // create
                model = new ApplicationUser();
            }
            else
            {
                // edit
                model = await _userManager.FindByIdAsync(id);
            }
            return View(model);
        } 
        
        [HttpPost]
        [Route("Create", Name = "UserCreate")]
        [Route("{id}", Name = "UserEdit")]
        public async Task<IActionResult> CreateOrEdit(string id, [FromForm] ApplicationUser model)
        {
            var dbModel = new ApplicationUser {Id = id};
            if (string.IsNullOrEmpty(id))
            {
                dbModel.Id = Guid.NewGuid().ToString();
                await _userManager.CreateAsync(new ApplicationUser
                {
                    Id = dbModel.Id,
                    UserName = model.UserName,
                    VoteIsRequired = model.VoteIsRequired
                });
            }
            else
            {
                dbModel = await _userManager.FindByIdAsync(id);
                dbModel.UserName = model.UserName;
                dbModel.VoteIsRequired = model.VoteIsRequired;
                await _userManager.UpdateAsync(dbModel);
            }
            
            return RedirectToRoute("UserEdit", new {id = dbModel.Id});
        }
    }
}