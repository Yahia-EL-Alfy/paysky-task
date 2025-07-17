# paysky-task Backend - Getting Started

This guide explains how to set up, configure, and run the paysky-task backend API project from scratch.

---

## Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (local or remote)
- (Optional) [Visual Studio 2022+](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)

---

## 1. Clone the Repository

---

## 2. Configure the Database Connection
Edit `appsettings.json` and set your SQL Server connection string:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=paysky_task_db;Trusted_Connection=True;TrustServerCertificate=True;"
}
```
- Replace `YOUR_SERVER` with your SQL Server instance name.

---

## 3. Apply Database Migrations
If this is your first time running the app, create the database schema:
```sh
dotnet tool restore # (if needed)
dotnet ef database update --project paysky-task.csproj
```
- This will create the database and all tables.

---

## 4. Run the Application
```sh
dotnet run --project paysky-task.csproj
```
- The API will start (by default on `https://localhost:7291` for swagger ).

---


## 5. Authentication
- Register a user via `POST /api/Auth/register` (role: `Employer` or `Applicant`).
- Login via `POST /api/Auth/login` to get a JWT token.
- Use the JWT token as a Bearer token for protected endpoints.

---

## 6. Background Services
- The app includes a background service that automatically archives expired vacancies every hour. No manual action is needed.

---

## 7. Troubleshooting
- If you get migration errors, ensure your connection string is correct and SQL Server is running.
- If you change the models, run:
  ```sh
  dotnet ef migrations add YourMigrationName --project paysky-task.csproj
  dotnet ef database update --project paysky-task.csproj
  ```
- For any issues, check the logs in the console output.

---

## 8. Useful Commands
- Run the app: `dotnet run --project paysky-task.csproj`
- Add migration: `dotnet ef migrations add MigrationName --project paysky-task.csproj`
- Update DB: `dotnet ef database update --project paysky-task.csproj`

---

## 9. Project Structure
- See `BACKEND_README.pdf` for a detailed explanation of every file and the architecture.

---

**You're ready to go!**
