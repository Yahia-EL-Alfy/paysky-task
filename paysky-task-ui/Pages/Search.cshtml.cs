using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using System;

namespace paysky_task_ui.Pages
{
    public class SearchModel : PageModel
    {
        public List<VacancyViewModel> Vacancies { get; set; }
        public string Message { get; set; }
        [BindProperty(SupportsGet = true)]
        public string Query { get; set; }
        [BindProperty]
        public int VacancyId { get; set; }
        public string Token { get; set; }

        public async Task OnGetAsync()
        {
            using var client = new HttpClient();
            var url = "https://localhost:5001/api/Application/search";
            if (!string.IsNullOrEmpty(Query))
                url += $"?query={Query}";
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                Vacancies = JsonSerializer.Deserialize<List<VacancyViewModel>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            else
            {
                Message = "Failed to load vacancies.";
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Token = Request.Cookies["jwt_token"];
            if (string.IsNullOrEmpty(Token))
            {
                Message = "Please login as Applicant.";
                await OnGetAsync();
                return Page();
            }
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
            var response = await client.PostAsJsonAsync("https://localhost:5001/api/Application/apply", new { VacancyId });
            if (response.IsSuccessStatusCode)
            {
                Message = "Application submitted!";
            }
            else
            {
                Message = "Failed to apply.";
            }
            await OnGetAsync();
            return Page();
        }
    }

    public class VacancyViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int MaxApplications { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsActive { get; set; }
    }
}
