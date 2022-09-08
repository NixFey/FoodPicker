using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Linq;
using System.Threading.Tasks;
using FoodPicker.Infrastructure.Data;
using FoodPicker.Infrastructure.Models;
using FoodPicker.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace FoodPicker.Web.Areas.Identity.Pages.Account.Manage
{
    public partial class AutoVoteModel : PageModel
    {
        private readonly MealVoteService _voteService;
        private readonly AutoVoteRepository _autoVoteRepo;
        private readonly UserManager<ApplicationUser> _userManager;
        
        [TempData]
        public string StatusMessage { get; set; }
        public IReadOnlyList<VoteOption> VoteOptions { get; set; }

        [BindProperty]
        public Dictionary<int, List<string>> AutoVotesViewModel { get; set; } = new();

        public List<AutoVote> AutoVotes { get; set; }
        
        public AutoVoteModel(MealVoteService voteService, UserManager<ApplicationUser> userManager, AutoVoteRepository autoVoteRepo)
        {
            _voteService = voteService;
            _userManager = userManager;
            _autoVoteRepo = autoVoteRepo;
        }
        
        private async Task LoadAsync(ApplicationUser user)
        {
            VoteOptions = await _voteService.GetAllVoteOptions();
            AutoVotesViewModel =
                new Dictionary<int, List<string>>(VoteOptions.Select(x =>
                    new KeyValuePair<int, List<string>>(x.Id, new())));

            AutoVotes = await _autoVoteRepo.GetAutoVotesForUser(user.Id);

            foreach (var autoVote in AutoVotes)
            {
                if (!AutoVotesViewModel.ContainsKey(autoVote.VoteOptionId))
                    AutoVotesViewModel[autoVote.VoteOptionId] = new List<string>();
                
                AutoVotesViewModel[autoVote.VoteOptionId].Add(autoVote.Keyword);
            }
        }
        
        public async Task<IActionResult> OnGetAsync()
        {
            await LoadAsync(await _userManager.GetUserAsync(User));
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            if (AutoVotesViewModel.ContainsKey(99999)) AutoVotesViewModel.Remove(99999);
            
            // This is a bit of a bad thing to do, but I'm doing it anyway.
            await _autoVoteRepo.DeleteRangeAsync(await _autoVoteRepo.GetAutoVotesForUser(user.Id));

            foreach (var voteOptionId in AutoVotesViewModel.Keys)
            {
                var autoVotes = AutoVotesViewModel[voteOptionId];
                await _autoVoteRepo.AddRangeAsync(autoVotes.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => new AutoVote()
                {
                    Keyword = x,
                    VoteOptionId = voteOptionId,
                    UserId = user.Id
                }));
            }

            await _voteService.ProcessAutoVotes(null, user.Id);
            await LoadAsync(user);
            return Page();
        }
    }
}
