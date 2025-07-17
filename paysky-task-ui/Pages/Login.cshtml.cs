using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Text.Json;

namespace paysky_task_ui.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public LoginDto LoginDto { get; set; }
        public string Message { get; set; }
        public string Token { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            using var client = new HttpClient();
            var response = await client.PostAsJsonAsync("https://localhost:5001/api/Auth/login", LoginDto);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(json);
                Token = doc.RootElement.GetProperty("token").GetString();
                Message = "Login successful!";
            }
            else
            {
                Message = "Login failed.";
            }
            return Page();
        }
    }

    public class LoginDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
