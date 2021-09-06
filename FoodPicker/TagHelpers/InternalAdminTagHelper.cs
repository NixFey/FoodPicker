using System.Security.Claims;
using System.Threading.Tasks;
using FoodPicker.Enums;
using FoodPicker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FoodPicker.TagHelpers
{
    [HtmlTargetElement("internal-admin")]
    public class InternalAdminTagHelper : TagHelper
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IActionContextAccessor _contextAccessor;
        public InternalAdminTagHelper(IAuthorizationService authorizationService, IActionContextAccessor contextAccessor)
        {
            _authorizationService = authorizationService;
            _contextAccessor = contextAccessor;
        }
        
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var currentUser = _contextAccessor.ActionContext.HttpContext.User;
            if (!(await _authorizationService.AuthorizeAsync(currentUser, AuthorizationPolicies.AccessInternalAdminAreas)).Succeeded)
            {
                output.SuppressOutput();
            }
        }
    }
}