﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MangerInstructions.Models;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

namespace MangerInstructions.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {

        private AccountDbContext accountDbContext;

        public AdminController(AccountDbContext accountDbContext)
        {
            this.accountDbContext = accountDbContext;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userId = User.Claims.FirstOrDefault(t => t.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId != null)
            {
                var user = accountDbContext.Users.FirstOrDefault(u => u.Id == userId);
                if (user == null)
                    HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).GetAwaiter();
                else if (user.IsBlock && user.Role != Role.Admin)
                {
                    HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).GetAwaiter();
                    context.Result = RedirectToAction("Block", "Account");
                }
            }
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> AssignAs(String Id)
        {
            var user = accountDbContext.Users.FirstOrDefault(u => u.Id == Id);
            if (user != null)
            {
                if (user.Role == Role.Admin)
                    user.Role = Role.User;
                else
                    user.Role = Role.Admin;
                await accountDbContext.SaveChangesAsync();
            }
            return Ok();
        }

        public async Task<IActionResult> DeleteAccount(String Id)
        {
            var user = accountDbContext.Users.FirstOrDefault(u => u.Id == Id);
            if (user != null)
            {
                user.Comments.Clear();
                user.Votes.Clear();
                user.Likes.Clear();
                accountDbContext.Users.Remove(user);
                await accountDbContext.SaveChangesAsync();
            }
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> BlockUser(String Id)
        {
            var user = accountDbContext.Users.FirstOrDefault(u => u.Id == Id);
            if (user != null)
            {
                user.IsBlock = !user.IsBlock;
                await accountDbContext.SaveChangesAsync();
            }
            return Ok();
        }

        public async Task<IActionResult> ChangeUserEmail(String Id, String email)
        {
            var user = accountDbContext.Users.FirstOrDefault(u => u.Id == Id);
            if (user != null && !String.IsNullOrEmpty(email))
            {
                user.Email = email;
                await accountDbContext.SaveChangesAsync();
            }
            return Ok();
        }

        public async Task<IActionResult> ChangeUserName(String Id, String name)
        {
            var user = accountDbContext.Users.FirstOrDefault(u => u.Id == Id);
            if (user != null && !String.IsNullOrEmpty(name))
            {
                user.Name = name;
                await accountDbContext.SaveChangesAsync();
            }
            return Ok();
        }
    }
}