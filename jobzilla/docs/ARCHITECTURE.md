# Jobzilla Dynamic Architecture

## Selected Design Direction

The strongest source design is the main `index.html` experience: it includes the marketplace search, categories, featured jobs, candidate/employer entry points, testimonials, blog content, newsletter, and modal authentication flows. The MVC layout now uses that design language and shared assets while moving data into SQL Server.

The complete original HTML template is also preserved under `src/Jobzilla.Web/wwwroot/template`. This keeps every static module available exactly as it was in the source HTML while the dynamic MVC implementation is migrated page by page.

## Clean Architecture Projects

- `Jobzilla.Domain`: business entities and enums only.
- `Jobzilla.Application`: use-case services, DTOs, queries, and database abstractions.
- `Jobzilla.Infrastructure`: EF Core SQL Server, ASP.NET Core Identity, role seeding, and persistence.
- `Jobzilla.Web`: MVC controllers, views, Identity UI, authorization policies, and static Jobzilla assets.

## Roles And Authorization

- `Candidate`: search jobs, save jobs, apply, upload/manage resumes, alerts, messages, profile.
- `Employer`: company profile, post/manage jobs, view applicants/resumes, packages, transactions, messages.
- `Admin`: manage users, roles, employers, candidates, job approvals, plans, payments, blog/CMS content.

Current policies:

- `AdminOnly`
- `EmployerOnly`
- `CandidateOnly`

## Entity Map From HTML Screens

- Job pages: `JobPost`, `JobCategory`, `Skill`, `JobPostSkill`.
- Candidate dashboard/resume pages: `CandidateProfile`, `CandidateResume`, `CandidateSkill`.
- Apply/saved/applied pages: `JobApplication`, `SavedJob`.
- Alerts pages: `JobAlert`.
- Employer profile/post/manage pages: `EmployerProfile`, `JobPost`.
- Pricing/package/transaction pages: `SubscriptionPlan`, `EmployerSubscription`, `PaymentTransaction`.
- Chat/message pages: `Conversation`, `Message`.
- Blog pages: `BlogPost`.
- About/contact/FAQ/static pages: `ContentPage`.
- Login/signup/change password pages: ASP.NET Core Identity with `ApplicationUser`, `IdentityRole`, and role policies.

## Static Template Modules

The `ModulesController` discovers every top-level `.html` file in `wwwroot/template` and exposes them from `/Modules`. Each module opens the preserved static HTML page, with its original relative resources copied beside it:

- `wwwroot/template/css`
- `wwwroot/template/js`
- `wwwroot/template/images`
- `wwwroot/template/img`
- `wwwroot/template/fonts`
- `wwwroot/template/files`
- `wwwroot/template/Trident`

## Database

The app uses SQL Server LocalDB by default:

```json
"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=JobzillaDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
```

Initial migration:

- `src/Jobzilla.Infrastructure/Persistence/Migrations/*_InitialJobzillaSchema.cs`

Seed data includes sample categories, subscription plans, one demo employer, and two published jobs.
