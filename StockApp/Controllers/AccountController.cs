using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models;
using StockApp.Controllers;
using StockApp.Core.Domain.Identity_Entities;
using StockApp.Core.DTO;
using StockApp.Core.Enums;

namespace StockApp.UI.Controllers
{
    [Route("[Controller]/[Action]")]
    //to make the controller available for people who didn't signin
    //[AllowAnonymous]
    public class AccountController : Controller
    {
        //userManager acts like a service we have to inject
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        public AccountController(UserManager<ApplicationUser> userManager,SignInManager<ApplicationUser> signInManager,RoleManager<ApplicationRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }
        [HttpGet]
        [Authorize("UnAuthorized")]
        public IActionResult Register()
        {
            ViewBag.path = "Register";
            ViewBag.dep = "Account";
            return View();
        }
        [HttpPost]
        [Authorize("UnAuthorized")]
        //[AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Register(RegisterDTO registerDTO)
        {
            ViewBag.path = "Register";
            ViewBag.dep = "Account";
            //validate the data 
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(value => value.Errors).Select(error => error.ErrorMessage).ToList();
                ViewBag.errors = errors;
                return View(registerDTO);
            }
            ApplicationUser applicationUser = new ApplicationUser()
            {
                PersonName = registerDTO.PersonName,
                Email = registerDTO.Email,
                UserName = registerDTO.Email,
                PhoneNumber = registerDTO.Phone
            };
            IdentityResult result = await _userManager.CreateAsync(applicationUser,registerDTO.Password);
            if (result.Succeeded)
            {
                //check if userType is Admin from radio Button
                if(registerDTO.UserTypeOptions == UserTypeOptions.Admin)
                {
                    //create admin role
                    if(await _roleManager.FindByNameAsync(registerDTO.UserTypeOptions.ToString()) is null)
                    {
                        ApplicationRole applicationRole = new ApplicationRole() { Name=registerDTO.UserTypeOptions.ToString() };
                        await _roleManager.CreateAsync(applicationRole);
                    }
                    //add the new user to admin role
                    await _userManager.AddToRoleAsync(applicationUser ,registerDTO.UserTypeOptions.ToString());
                }
                await _signInManager.SignInAsync(applicationUser,isPersistent: false);
                return RedirectToAction("Index","Trade");
            }
            else
            {
                foreach(IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("Register", error.Description);
                }
                return View(registerDTO);
            }
        }
        [HttpGet]
        [Authorize("UnAuthorized")]
        public IActionResult LogIn()
        {
            ViewBag.path = "LogIn";
            ViewBag.dep = "Account";
            return View();
        }
        [HttpPost]
        [Authorize("UnAuthorized")]
        public async Task<IActionResult> LogIn(LoginDTO loginDTO, string? ReturnUrl)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(value => value.Errors).Select(error => error.ErrorMessage).ToList();
                ViewBag.errors = errors;
                return View(loginDTO);
            }
            Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(loginDTO.Email, loginDTO.Password,false,false);
            if (result.Succeeded)
            {
                ApplicationUser applicationUser = await _userManager.FindByEmailAsync(loginDTO.Email);
                if(applicationUser != null)
                {
                    if(await _userManager.IsInRoleAsync(applicationUser, UserTypeOptions.Admin.ToString()))
                    {
                        return RedirectToAction("Index", "Home", new { area = "Admin" });
                    }
                }
                if(!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                {
                    return LocalRedirect(ReturnUrl);
                }
                return RedirectToAction("Index", "Trade");
            }
            else
            {
                ModelState.AddModelError("LogIn", "Invalid Email or Password");
                return View(loginDTO);
            }
        }
        public async Task<IActionResult> LogOut()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Trade");
        }
        public async Task<IActionResult> IsEmailRegistered(string email)
        {
            var result = await _userManager.FindByEmailAsync(email);
            if(email == null)
            {
                return Json(false);
            }
            else
            {
                return Json(true);
            }
        }
    }
}
