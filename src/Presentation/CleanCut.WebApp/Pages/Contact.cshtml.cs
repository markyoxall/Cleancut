using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CleanCut.WebApp.Models.Customers;

namespace CleanCut.WebApp.Pages
{
    public class ContactModel : PageModel
    {
        [BindProperty]
        public CustomerEditViewModel Input { get; set; } = new();

        public void OnGet()
        {
        }

        [ValidateAntiForgeryToken]
        public IActionResult OnPost()
        {
            var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            if (!ModelState.IsValid)
            {
                if (isAjax)
                {
                    var errors = ModelState
                        .Where(kvp => kvp.Value.Errors.Count > 0)
                        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray());

                    return BadRequest(new { errors });
                }

                return Page();
            }

            // TODO: handle submission (send email, persist, etc.)
            TempData["SuccessMessage"] = "Thanks for contacting us.";

            if (isAjax)
            {
                return new JsonResult(new { success = true, message = "Thanks for contacting us." });
            }

            return RedirectToPage();
        }
    }
}
