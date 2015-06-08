using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using ChinaTea.Models;
using ChinaTea.Areas.Admin.Models;


namespace ChinaTea.Areas.Admin.Controllers
{
    [Authorize(Roles="Administrators")]
    
    public class AdminAccountController : Controller
    {
        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager;

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
        }

        // GET: Admin/AdminAccount
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult SearchUsers(string searchText, int page = 1)
        {
            UsersViewModel vm = new UsersViewModel();
            List<ApplicationUser> users = new List<ApplicationUser>();
            if (searchText == null)
            {
                return View();
            }
            if (searchText == "All")
            {
                users = UserManager.Users.ToList();
            }
            else
            {
                users = UserManager.Users.Where(u => u.UserName.Contains(searchText) || u.Email.Contains(searchText)).ToList();

            }
            PageInfo pageInfo = new PageInfo() { PageSize = 20, CurrentPage = 1 };
            pageInfo.PageCount = (users.Count() - 1) / pageInfo.PageSize + 1;
            pageInfo.CurrentPage = page <= pageInfo.PageCount ? page : 1;
            pageInfo.Url = "/admin/adminaccount/searchUsers?searchText=" + searchText + "@page=";
            vm.Users = users.Skip((pageInfo.CurrentPage - 1) * pageInfo.PageSize).Take(pageInfo.PageSize).ToList();
            vm.PageInfo = pageInfo;
            return View(vm);
        }

        public ActionResult SearchRoles(string searchText)
        {
            List<ApplicationRole> roles = new List<ApplicationRole>();
            if (searchText == null)
            {
                return View();
            }
            if (searchText == "All")
            {
                roles = RoleManager.Roles.ToList();
            }
            else
            {
                roles = RoleManager.Roles.Where(r => r.Name.Contains("searchText")).ToList();
            }
            return View(roles);
        }

        public ActionResult CreateRole()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> CreateRole([Required] string name)
        {
            if (ModelState.IsValid)
            {
                IdentityResult result = await RoleManager.CreateAsync(new ApplicationRole(name));
                if (result.Succeeded)
                {
                    return RedirectToAction("searchRoles", new { searchtext = "All" });

                }
                else
                {
                    AddErrorsFromResult(result);
                }

            }
            return View(name);
        }

        public async Task<ActionResult> EditRole(string id)
        {
            ApplicationRole role = await RoleManager.FindByIdAsync(id);
            if (role == null)
            {
                TempData.Add("Error", "Error, Role Not Found!");
                return RedirectToAction("searchRoles", new { searchtext = "All" });
            }
            else
            {
                string[] memberIDs = role.Users.Select(u => u.UserId).ToArray();
                IEnumerable<ApplicationUser> members = UserManager.Users.Where(x => memberIDs.Any(y => y == x.Id));
                IEnumerable<ApplicationUser> nonMenbers = UserManager.Users.Except(members);
                return View(new RoleEditModel() { Role = role, Members = members, NonMembers = nonMenbers });
            }
        }
        [HttpPost]
        public async Task<ActionResult> EditRole(RoleModificationModel model)
        {
            IdentityResult result;
            if (ModelState.IsValid)
            {
                foreach (string userId in model.IdsToAdd ?? new string[] { })
                {
                    result = await UserManager.AddToRoleAsync(userId, model.RoleName);
                    if (!result.Succeeded)
                    {
                        return View("Error", result.Errors);
                    }
                }
                foreach (var userId in model.IdsToRemove ?? new string[] { })
                {
                    result = await UserManager.RemoveFromRoleAsync(userId, model.RoleName);
                    if (!result.Succeeded)
                    {
                        return View("Error", result.Errors);
                    }
                }
                return RedirectToAction("SearchRoles", new { searchText = "All" });
            }
            return View("Error", new string[] { "Role Not Found" });
        }

        public async Task<ActionResult> DeleteRole(string id)
        {
            ApplicationRole role = await RoleManager.FindByIdAsync(id);
            if (role != null)
            {
                IdentityResult result = await RoleManager.DeleteAsync(role);
                if (result.Succeeded)
                {
                    return RedirectToAction("SearchRoles", new { searchText = "All" });
                }
                else
                {
                    return View("Error", result.Errors);
                }
            }
            else
            {
                return View("Error", new String[] { "Role Not Found" });
            }
        }

        public async Task<ActionResult> DeleteUser(string id)
        {
            ApplicationUser user = await UserManager.FindByIdAsync(id);
            if (user != null)
            {
                IdentityResult result = await UserManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction("SearchUsers", new { searchText = "All" });
                }
                else
                {
                    return View("Error", result.Errors);
                }
            }
            else
            {
                return View("Error", new String[] { "User Not Found" });
            }
        }

        public async Task<ActionResult> EditUser(string id)
        {
            ApplicationUser user = await UserManager.FindByIdAsync(id);
            if (user!=null)
            {
                EditUserViewModel userVm = new EditUserViewModel()
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    Email = user.Email,
                    LastName = user.LastName,
                    Password = null,
                    ConfirmPassword = null
                };
                return View(userVm);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditUser(ChinaTea.Models.EditUserViewModel userVm)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = await UserManager.FindByIdAsync(userVm.Id);
                if (user!=null)
                {
                    user.Email = userVm.Email;
                    user.UserName = userVm.Email;
                    user.FirstName = userVm.FirstName;
                    user.LastName = userVm.LastName;
                    IdentityResult validEmail = await UserManager.UserValidator.ValidateAsync(user);
                    if (!validEmail.Succeeded)
                    {
                        AddErrorsFromResult(validEmail);
                    }
                    IdentityResult validPass = null;
                    if (!string.IsNullOrEmpty(userVm.Password))
                    {
                        validPass = await UserManager.PasswordValidator.ValidateAsync(userVm.Password);
                        if(validPass.Succeeded)
                        {
                            user.PasswordHash = UserManager.PasswordHasher.HashPassword(userVm.Password);
                        }
                        else
                        {
                            AddErrorsFromResult(validPass);
                        }
                    }
                    if((validEmail.Succeeded &&validPass==null) || (validEmail.Succeeded&&(!string.IsNullOrEmpty(userVm.Password))&&validPass.Succeeded))
                    {
                        IdentityResult result = await UserManager.UpdateAsync(user);
                        if (result.Succeeded)
                        {
                            return RedirectToAction("searchusers", new { searchText = "All" });
                        }
                        else
                        {
                            AddErrorsFromResult(result);
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("","User Not Found");
                }
            }
            return View(userVm);
        }
        
        private void AddErrorsFromResult(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }
    }
}