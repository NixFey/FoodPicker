using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using FoodPicker.Infrastructure.Data;
using FoodPicker.Web.Enums;
using FoodPicker.Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FoodPicker.Web.Controllers
{
    [Route("[controller]")]
    [Authorize(Policy = AuthorizationPolicies.AccessInternalAdminAreas)]
    public class OptionController : Controller
    {
        private readonly EfRepository<VoteOption> _optionRepo;

        public OptionController(EfRepository<VoteOption> optionRepo)
        {
            _optionRepo = optionRepo;
        }
        
        public async Task<IActionResult> Index()
        {
            var options = await _optionRepo.ListAllAsync();
            return View("List", options);
        }
        
        [HttpGet]
        [Route("Create")]
        [Route("{id:int?}")]
        public async Task<IActionResult> CreateOrEdit(int? id)
        {
            VoteOption model;
            var isCreating = id is null or 0;
            if (isCreating)
            {
                model = new VoteOption();
            }
            else
            {
                // edit
                model = await _optionRepo.GetByIdAsync((int) id);
                if (model == null) return NotFound();
            }
            return View(model);
        } 
        
        [HttpPost]
        [Route("Create", Name = "OptionCreate")]
        [Route("{id:int?}", Name = "OptionEdit")]
        public async Task<IActionResult> CreateOrEdit(int? id, [FromForm] VoteOption model)
        {
            if (!ModelState.IsValid) return View(model);
            
            VoteOption dbModel;
            var isCreating = id is null or 0;
            if (isCreating)
            {
                dbModel = new VoteOption
                {
                    Name = model.Name,
                    Weight = model.Weight
                };
                await _optionRepo.AddAsync(dbModel);
            }
            else
            {
                dbModel = await _optionRepo.GetByIdAsync((int) id);
                dbModel.Name = model.Name;
                dbModel.Weight = model.Weight;
                await _optionRepo.UpdateAsync(dbModel);
            }

            return RedirectToRoute("OptionEdit", new {id = dbModel.Id});
        }
    }
}