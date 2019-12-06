using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MangerInstructions.Models;
using Korzh.EasyQuery.Linq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using MangerInstructions.ViewModel;

namespace MangerInstructions.Controllers
{
    [Authorize]
    [Route("Profile")]
    public class UserPageController : Controller
    {
        private MangerInstructionsDbContext mangerInstructionsDbContext;
        private readonly IStringLocalizer<SharedResource> sharedLocalizer;
        private IHostingEnvironment appEnvironment;

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userId = User.Claims.FirstOrDefault(t => t.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId != null)
            {
                var user = mangerInstructionsDbContext.Users.FirstOrDefault(u => u.Id == userId);
                if (user == null)
                    HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).GetAwaiter();
                else if (user.IsBlock)
                {
                    HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).GetAwaiter();
                    context.Result = RedirectToAction("Block", "Account");
                }
            }
        }

        public UserPageController(MangerInstructionsDbContext context, IStringLocalizer<SharedResource> sharedLocalizer, IHostingEnvironment appEnvironment)
        {
            mangerInstructionsDbContext = context;
            this.sharedLocalizer = sharedLocalizer;
            this.appEnvironment = appEnvironment;
        }

        [Route("Search")]
        public async Task<IActionResult> SearchUserPage(String textSearch, String Id)
        {
            var userId = User.Claims.FirstOrDefault(t => t.Type == ClaimTypes.NameIdentifier)?.Value;
            var user = await mangerInstructionsDbContext.Users.FirstOrDefaultAsync(u => u.Id == Id);
            if (!String.IsNullOrEmpty(textSearch) && user != null)
            {
                var instructions = mangerInstructionsDbContext.Instructions.Where(i => i.Author.Id == Id).FullTextSearchQuery(textSearch).ToList();
                user.PersonalPage.Instructions.Clear();
                user.PersonalPage.Instructions = instructions;
            }
            ViewBag.Owner = (Id == userId);
            return View("UserPage", user);
        }

        [Route("ByRating")]
        public async Task<IActionResult> ByRatingForUser(String Id)
        {
            var userId = User.Claims.FirstOrDefault(t => t.Type == ClaimTypes.NameIdentifier)?.Value;
            var user = await mangerInstructionsDbContext.Users.FirstOrDefaultAsync(u => u.Id == Id);
            if (user == null)
                return RedirectToAction("Index");
            ViewBag.Owner = (Id == userId);
            user.PersonalPage.Instructions = user.PersonalPage.Instructions.OrderByDescending(r => r.GetRating()).ToList();
            return View("UserPage", user);
        }

        [Route("Newest")]
        public async Task<IActionResult> NewestForUser(String Id)
        {
            var userId = User.Claims.FirstOrDefault(t => t.Type == ClaimTypes.NameIdentifier)?.Value;
            var user = await mangerInstructionsDbContext.Users.FirstOrDefaultAsync(u => u.Id == Id);
            if (user == null)
                return RedirectToAction("Index");
            ViewBag.Owner = (Id == userId);
            user.PersonalPage.Instructions = user.PersonalPage.Instructions.OrderByDescending(r => r.DateTime).ToList();
            return View("UserPage", user);
        }

        [Route("Older")]
        public async Task<IActionResult> OlderForUser(String Id)
        {
            var userId = User.Claims.FirstOrDefault(t => t.Type == ClaimTypes.NameIdentifier)?.Value;
            var user = await mangerInstructionsDbContext.Users.FirstOrDefaultAsync(u => u.Id == Id);
            if (user == null)
                return RedirectToAction("Index");
            ViewBag.Owner = (Id == userId);
            user.PersonalPage.Instructions = user.PersonalPage.Instructions.OrderBy(r => r.DateTime).ToList();
            return View("UserPage", user);
        }

        [HttpGet]
        [Route("{id:guid}")]
        public IActionResult UserPage(String Id)
        {
            var userId = User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.NameIdentifier)?.Value;
            ViewBag.Owner = (Id == userId);
            var user = mangerInstructionsDbContext.Users.FirstOrDefault(u => u.Id == Id);
            return View(user);
        }

        [HttpPost]
        [Route("Added")]
        public IActionResult AddStep(InstructionViewModel instructionViewModel)
        {
            UpdateImage(instructionViewModel);
            instructionViewModel.Steps.Add(new StepInstructionViewModel());
            ViewBag.Categories = GetCategories();
            return View("Edit", instructionViewModel);
        }

        [HttpPost]
        [Route("Deleted")]
        public IActionResult DeleteStep(InstructionViewModel instructionViewModel)
        {
            int lastIndex = instructionViewModel.Steps.Count - 1;
            UpdateImage(instructionViewModel);
            DeleteImage(instructionViewModel.Steps[lastIndex].ImageLinks);
            instructionViewModel.Steps.RemoveAt(lastIndex);
            ViewBag.Categories = GetCategories();
            return View("Edit", instructionViewModel);
        }

        [HttpPost]
        [Route("Created")]
        public async Task<IActionResult> CreateInstruction(InstructionViewModel instructionViewModel)
        {
            var userId = User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.NameIdentifier).Value;
            var user = await mangerInstructionsDbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            UpdateImage(instructionViewModel);
            Instruction newInstruction = ConvertToInstuction(instructionViewModel);
            newInstruction.Author = user;
            if (TempData["InstructionId"] != null)
            {
                String instructionId = TempData["InstructionId"] as String;
                TempData.Clear();
                Instruction oldInstruction = await mangerInstructionsDbContext.Instructions.FirstOrDefaultAsync(i => i.Id == instructionId);
                newInstruction.Comments = oldInstruction.Comments;
                newInstruction.Votes.AddRange(oldInstruction.Votes);
                user.PersonalPage.AddInstruction(newInstruction);
                user.PersonalPage.RemoveInstruction(oldInstruction);
            }
            else
                user.PersonalPage.AddInstruction(newInstruction);
            await mangerInstructionsDbContext.SaveChangesAsync();
            return RedirectToAction("UserPage", new { Id = userId });
        }

        [HttpGet]
        [Route("Create")]
        public IActionResult Create()
        {
            TempData.Clear();
            ViewBag.Categories = GetCategories();
            return View("Edit", new InstructionViewModel());
        }

        [HttpPost]
        [Route("Edit")]
        public async Task<IActionResult> Edit(String instructionId)
        {
            var instruction = await mangerInstructionsDbContext.Instructions.FirstOrDefaultAsync(i => i.Id == instructionId);
            ViewBag.Categories = GetCategories();
            ViewBag.Locale = false;
            TempData["InstructionId"] = instructionId;
            return View(new InstructionViewModel(instruction));
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IActionResult> Delete(String instructionId)
        {
            var instruction = await mangerInstructionsDbContext.Instructions.FirstOrDefaultAsync(i => i.Id == instructionId);
            var user = await mangerInstructionsDbContext.Users.FirstOrDefaultAsync(u => u.Id == instruction.Author.Id);
            foreach (var step in instruction.Steps)
                DeleteImage(step.ImageLinks);
            user.PersonalPage.RemoveInstruction(instruction);
            await mangerInstructionsDbContext.SaveChangesAsync();
            return RedirectToAction("UserPage", new { Id = user.Id });
        }

        private Instruction ConvertToInstuction(InstructionViewModel instructionViewModel)
        {
            Instruction instruction = new Instruction();
            instruction.Author = instructionViewModel.Author;
            instruction.Name = instructionViewModel.Name;
            instruction.CategoryIndex = instructionViewModel.Category;
            instruction.ShortDescription = instructionViewModel.ShortDescription;
            instruction.DateTime = DateTime.Now;
            foreach (var tag in instructionViewModel.Tags.Split(','))
                instruction.Tags.Add(new Tag { TagName = tag });
            foreach (var step in instructionViewModel.Steps)
            {
                instruction.AddStep(new StepInstruction
                {
                    Name = step.Name,
                    ImageLinks = step.ImageLinks,
                    Text = step.Text
                });
            }
            return instruction;
        }

        private void UpdateImage(InstructionViewModel instructionViewModel)
        {
            var userId = User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.NameIdentifier).Value;
            foreach (var step in instructionViewModel.Steps)
            {
                if (step.FormImages == null)
                    continue;
                DeleteImage(step.ImageLinks);
                step.ImageLinks = new List<String>();
                IFormFileCollection formFiles = step.FormImages;
                for (int i = 0; i < formFiles.Count && i < 3; ++i)
                {
                    IFormFile uploadedFile = formFiles[i];
                    String path = "/files/" + userId + "_" + DateTime.Now.Subtract(DateTime.MinValue).TotalSeconds.ToString() + uploadedFile.FileName;
                    using (var fileStream = new FileStream(appEnvironment.WebRootPath + path, FileMode.Create))
                    {
                        uploadedFile.CopyTo(fileStream);
                    }
                    step.ImageLinks.Add(path);
                }
            }
        }

        private void DeleteImage(List<String> imageLinks)
        {
            var path = appEnvironment.WebRootPath;
            foreach (var imageLink in imageLinks)
            {
                if (System.IO.File.Exists(path + imageLink))
                    System.IO.File.Delete(path + imageLink);
            }
        }

        private List<SelectListItem> GetCategories()
        {
            var categiriesIndex = mangerInstructionsDbContext.Categories.ToArray();
            List<SelectListItem> categories = new List<SelectListItem>();
            categories.Add(new SelectListItem { Value = "", Selected = true });
            for (int i = 0; i < categiriesIndex.Length; ++i)
                categories.Add(new SelectListItem { Value = categiriesIndex[i].Index, Text = sharedLocalizer[categiriesIndex[i].Index] });
            return categories;
        }
    }
}