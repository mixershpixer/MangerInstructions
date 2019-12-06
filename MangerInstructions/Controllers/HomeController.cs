using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MangerInstructions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Korzh.EasyQuery.Linq;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Diagnostics;

namespace MangerInstructions.Controllers
{
    public class HomeController : Controller
    {
        private MangerInstructionsDbContext mangerInstructionsDbContext;

        public HomeController(MangerInstructionsDbContext mangerInstructionsDbContext)
        {
            this.mangerInstructionsDbContext = mangerInstructionsDbContext;
        }

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

        [Route("Search")]
        public IActionResult Search(String text)
        {
            HashSet<Instruction> instructions = new HashSet<Instruction>();
            if (!String.IsNullOrEmpty(text))
            {
                instructions = mangerInstructionsDbContext.Instructions.FullTextSearchQuery(text).ToHashSet();
                var findByCommentInstructions = mangerInstructionsDbContext.Comments.FullTextSearchQuery(text).Select(i => i.Instruction).ToHashSet();
                instructions.UnionWith(findByCommentInstructions);
            }
            ViewBag.Category = GetCategory();
            ViewBag.Tags = GetTopTags();
            return View("Index", instructions.ToList());
        }

        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.Category = GetCategory();
            ViewBag.Tags = GetTopTags();
            return View(mangerInstructionsDbContext.Instructions.OrderByDescending(t => t.DateTime).ToList());
        }

        [Route("ByRating")]
        public IActionResult ByRating()
        {
            ViewBag.Category = GetCategory();
            ViewBag.Tags = GetTopTags();
            return View("Index", mangerInstructionsDbContext.Instructions.ToList().OrderByDescending(r => r.GetRating()));
        }

        [Route("Newest")]
        public IActionResult Newest()
        {
            ViewBag.Category = GetCategory();
            ViewBag.Tags = GetTopTags();
            return View("Index", mangerInstructionsDbContext.Instructions.OrderByDescending(t => t.DateTime));
        }

        [Route("Older")]
        public IActionResult Older()
        {
            ViewBag.Category = GetCategory();
            ViewBag.Tags = GetTopTags();
            return View("Index", mangerInstructionsDbContext.Instructions.OrderBy(t => t.DateTime));
        }

        [Route("SearchByTag")]
        public IActionResult SearchByTag(String tag)
        {
            var instructions = mangerInstructionsDbContext.Instructions.Where(t => t.Tags.Any(n => n.TagName == tag)).ToList();
            ViewBag.Category = GetCategory();
            ViewBag.Tags = GetTopTags();
            return View("Index", instructions);
        }

        [Route("SearchByCategory")]
        public IActionResult SearchByCategory(String index)
        {
            var instructions = mangerInstructionsDbContext.Instructions.Where(c => c.CategoryIndex == index).ToList();
            ViewBag.Category = GetCategory();
            ViewBag.Tags = GetTopTags();
            return View("Index", instructions);
        }

        public async Task<IActionResult> AddComment(String idInstruction, String text)
        {
            var instruction = await mangerInstructionsDbContext.Instructions.FirstOrDefaultAsync(i => i.Id == idInstruction);
            if (instruction != null)
            {
                var userId = User.Claims.FirstOrDefault(t => t.Type == ClaimTypes.NameIdentifier)?.Value;
                var user = await mangerInstructionsDbContext.Users.FirstAsync(u => u.Id == userId);
                instruction.AddComment(new Comment
                {
                    User = user,
                    Text = text,
                    Time = DateTime.Now
                });
                await mangerInstructionsDbContext.SaveChangesAsync();
            }
            return Ok();
        }

        public async Task<IActionResult> Like(String idComment)
        {
            var userId = User.Claims.FirstOrDefault(t => t.Type == ClaimTypes.NameIdentifier)?.Value;
            var user = await mangerInstructionsDbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            var comment = await mangerInstructionsDbContext.Comments.FirstOrDefaultAsync(c => c.Id == idComment);
            if (comment != null && user != null)
            {
                var like = comment.Likes.FirstOrDefault(l => l.User.Id == user.Id);
                if (like == null)
                    comment.Likes.Add(new Like { User = user });
                else
                    comment.Likes.Remove(like);
                await mangerInstructionsDbContext.SaveChangesAsync();
            }
            return Ok();
        }

        public async Task<IActionResult> LoadComments(String idInstruction)
        {
            var instruction = await mangerInstructionsDbContext.Instructions.FirstOrDefaultAsync(i => i.Id == idInstruction);
            return PartialView("_Comments", instruction.Comments.OrderBy(t => t.Time).ToList());
        }

        public async Task<IActionResult> RemoveComment(String idComment)
        {
            var comment = await mangerInstructionsDbContext.Comments.FirstOrDefaultAsync(c => c.Id == idComment);
            if (comment != null)
            {
                var userId = User.Claims.FirstOrDefault(t => t.Type == ClaimTypes.NameIdentifier)?.Value;
                var user = await mangerInstructionsDbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user != null && user.Id == comment.User.Id)
                {
                    var instruction = comment.Instruction;
                    instruction.RemoveComment(comment);
                    await mangerInstructionsDbContext.SaveChangesAsync();
                }
            }
            return Ok();
        }

        [HttpGet]
        [Route("Page/{id:guid}")]
        public IActionResult InstructionPage(String Id)
        {
            var instruction = mangerInstructionsDbContext.Instructions.FirstOrDefault(i => i.Id == Id);
            if (instruction != null)
                return View(instruction);
            else
                return RedirectToAction("Index");
        }

        public async Task<String> SetVote(String idInstruction, int rating)
        {
            var userId = User.Claims.FirstOrDefault(t => t.Type == ClaimTypes.NameIdentifier)?.Value;
            var user = await mangerInstructionsDbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            var instruction = await mangerInstructionsDbContext.Instructions.FirstOrDefaultAsync(i => i.Id == idInstruction);
            if (user != null && instruction != null)
            {
                if (instruction.Votes.Any(v => v.User.Id == userId))
                    instruction.RemoveVoteOfUser(userId);
                instruction.Votes.Add(new Vote { Rating = rating, User = user });
                await mangerInstructionsDbContext.SaveChangesAsync();
                return $"{instruction.GetRating():#.##}";
            }
            return "0";
        }

        [HttpPost]
        public IActionResult SetTheme(String theme)
        {
            Response.Cookies.Append("theme", theme, new CookieOptions { Expires = DateTime.Now.AddDays(30) });
            return Ok();
        }

        public async Task<String> GetVotes(String idInstruction)
        {
            var instruction = await mangerInstructionsDbContext.Instructions.FirstOrDefaultAsync(i => i.Id == idInstruction);
            if (instruction != null)
                return $"{instruction.GetRating():#.##}";
            else
                return "0";
        }

        [HttpPost]
        public IActionResult SetLanguage(String culture, String url)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTime.Now.AddDays(30) }
            );
            return Redirect(url);
        }

        private List<Category> GetCategory()
        {
            return mangerInstructionsDbContext.Categories.ToList(); ;
        }

        private List<KeyValuePair<String, int>> GetTopTags()
        {
            Dictionary<String, int> tags = new Dictionary<String, int>();
            var tagArray = mangerInstructionsDbContext.Tags.Select(n => n.TagName).ToArray();
            for (int i = 0; i < tagArray.Count(); ++i)
            {
                if (tags.ContainsKey(tagArray[i]))
                    tags[tagArray[i]]++;
                else
                    tags.Add(tagArray[i], 1);
            }
            return tags.OrderByDescending(v => v.Value).Take(16).ToList();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
