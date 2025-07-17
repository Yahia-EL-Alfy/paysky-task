using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace paysky_task_ui.Pages
{
    public class RegisterModel : PageModel
    {
        [BindProperty]
        public RegisterDto RegisterDto { get; set; }
        public string Message { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            using var client = new HttpClient();
            // Change the API URL as needed
            var response = await client.PostAsJsonAsync("https://localhost:5001/api/Auth/register", RegisterDto);
            if (response.IsSuccessStatusCode)
            {
                Message = "Registration successful!";
            }
            else
            {
                Message = "Registration failed.";
            }
            return Page();
        }
    }

    public class RegisterDto
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
    }
}
