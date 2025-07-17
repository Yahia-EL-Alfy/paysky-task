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
    public class VacanciesModel : PageModel
    {
        [BindProperty]
        public VacancyDto NewVacancy { get; set; }
        public List<VacancyViewModel> Vacancies { get; set; }
        public string Message { get; set; }
        public string Token { get; set; }

        public async Task OnGetAsync()
        {
            Token = Request.Cookies["jwt_token"];
            if (string.IsNullOrEmpty(Token))
            {
                Message = "Please login as Employer.";
                return;
            }
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
            var response = await client.GetAsync("https://localhost:5001/api/Vacancy");
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
                Message = "Please login as Employer.";
                return Page();
            }
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
            var response = await client.PostAsJsonAsync("https://localhost:5001/api/Vacancy", NewVacancy);
            if (response.IsSuccessStatusCode)
            {
                Message = "Vacancy created!";
                await OnGetAsync();
            }
            else
            {
                Message = "Failed to create vacancy.";
            }
            return Page();
        }
    }

    public class VacancyDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int MaxApplications { get; set; }
        public DateTime ExpiryDate { get; set; }
    }
}
