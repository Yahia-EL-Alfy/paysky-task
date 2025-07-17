namespace paysky_task.Entities
{
    public enum UserRole
    {
        Employer,
        Applicant
    }

    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public UserRole Role { get; set; }
        public ICollection<Application> Applications { get; set; }
        public ICollection<Vacancy> Vacancies { get; set; } // For Employers
    }

    public class Vacancy
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int EmployerId { get; set; }
        public User Employer { get; set; }
        public int MaxApplications { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsArchived { get; set; }
        public ICollection<Application> Applications { get; set; }
    }

    public class Application
    {
        public int Id { get; set; }
        public int ApplicantId { get; set; }
        public User Applicant { get; set; }
        public int VacancyId { get; set; }
        public Vacancy Vacancy { get; set; }
        public DateTime AppliedAt { get; set; }
    }
}
