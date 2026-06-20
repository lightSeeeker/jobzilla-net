using jobzilla_net.Application.Admin;
using jobzilla_net.Application.Admin.Dtos;
using jobzilla_net.Application.Common;
using jobzilla_net.Application.Common.Interfaces;
using jobzilla_net.Core.Entities;
using jobzilla_net.Core.Enums;
using jobzilla_net.Infrasture.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace jobzilla_net.Infrasture.Services;

public class AdminService : IAdminService
{
    private readonly IApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminService(IApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // Dashboard
    public async Task<AdminDashboardStatsDto> GetDashboardStatsAsync(CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;
        var weekAgo = today.AddDays(-7);
        var monthAgo = today.AddMonths(-1);

        var totalEmployers = await _context.EmployerProfiles.CountAsync(ct);
        var totalCandidates = await _context.CandidateProfiles.CountAsync(ct);
        var newUsersToday = await _context.EmployerProfiles.CountAsync(e => e.CreatedAtUtc.Date == today, cancellationToken: ct)
            + await _context.CandidateProfiles.CountAsync(c => c.CreatedAtUtc.Date == today, cancellationToken: ct);
        var newUsersThisWeek = await _context.EmployerProfiles.CountAsync(e => e.CreatedAtUtc.Date >= weekAgo, cancellationToken: ct)
            + await _context.CandidateProfiles.CountAsync(c => c.CreatedAtUtc.Date >= weekAgo, cancellationToken: ct);
        var newUsersThisMonth = await _context.EmployerProfiles.CountAsync(e => e.CreatedAtUtc.Date >= monthAgo, cancellationToken: ct)
            + await _context.CandidateProfiles.CountAsync(c => c.CreatedAtUtc.Date >= monthAgo, cancellationToken: ct);
        var activeUsers = await _userManager.Users.CountAsync(u => u.IsActive, cancellationToken: ct);
        var blockedUsers = await _userManager.Users.CountAsync(u => !u.IsActive, cancellationToken: ct);
        var verifiedUsers = await _context.EmployerProfiles.CountAsync(e => e.IsVerified, cancellationToken: ct);

        var totalJobs = await _context.JobPosts.CountAsync(ct);
        var activeJobs = await _context.JobPosts.CountAsync(j => j.Status == JobStatus.Published && j.ExpiresAtUtc > DateTime.UtcNow, ct);
        var expiredJobs = await _context.JobPosts.CountAsync(j => j.ExpiresAtUtc <= DateTime.UtcNow && j.Status == JobStatus.Published, ct);

        var totalResumes = await _context.CandidateResumes.CountAsync(ct);
        var totalDownloads = 0; // TODO: Track in separate downloads table

        var totalRevenue = await _context.PaymentTransactions
            .Where(p => p.Status == "Completed")
            .SumAsync(p => p.Amount, cancellationToken: ct);

        var currentMonth = DateTime.UtcNow.Month;
        var currentYear = DateTime.UtcNow.Year;
        var monthlyRevenue = await _context.PaymentTransactions
            .Where(p => p.Status == "Completed" && p.TransactionDateUtc.Month == currentMonth && p.TransactionDateUtc.Year == currentYear)
            .SumAsync(p => p.Amount, cancellationToken: ct);

        return new AdminDashboardStatsDto
        {
            TotalEmployers = totalEmployers,
            TotalCandidates = totalCandidates,
            NewUsersToday = newUsersToday,
            NewUsersThisWeek = newUsersThisWeek,
            NewUsersThisMonth = newUsersThisMonth,
            ActiveUsers = activeUsers,
            BlockedUsers = blockedUsers,
            VerifiedUsers = verifiedUsers,
            TotalJobsPosted = totalJobs,
            ActiveJobs = activeJobs,
            ExpiredJobs = expiredJobs,
            TotalResumes = totalResumes,
            TotalResumesDownloaded = totalDownloads,
            TotalRevenue = totalRevenue,
            MonthlyRevenue = monthlyRevenue
        };
    }

    public async Task<List<AdminActivityDto>> GetRecentActivitiesAsync(int count = 15, CancellationToken ct = default)
    {
        var activities = new List<AdminActivityDto>();

        // Recent job posts
        var recentJobs = await _context.JobPosts
            .OrderByDescending(j => j.CreatedAtUtc)
            .Take(count / 3)
            .ToListAsync(ct);

        activities.AddRange(recentJobs.Select(j => new AdminActivityDto
        {
            Type = "JobPosted",
            Description = $"New job posted",
            EntityName = j.Title,
            OccurredAt = j.CreatedAtUtc
        }));

        // Recent employer registrations
        var recentEmployers = await _context.EmployerProfiles
            .OrderByDescending(e => e.CreatedAtUtc)
            .Take(count / 3)
            .ToListAsync(ct);

        activities.AddRange(recentEmployers.Select(e => new AdminActivityDto
        {
            Type = "EmployerRegistered",
            Description = $"New employer registered",
            EntityName = e.CompanyName,
            OccurredAt = e.CreatedAtUtc
        }));

        // Recent candidate registrations
        var recentCandidates = await _context.CandidateProfiles
            .OrderByDescending(c => c.CreatedAtUtc)
            .Take(count / 3)
            .ToListAsync(ct);

        activities.AddRange(recentCandidates.Select(c => new AdminActivityDto
        {
            Type = "CandidateRegistered",
            Description = $"New candidate registered",
            EntityName = c.FullName,
            OccurredAt = c.CreatedAtUtc
        }));

        return activities.OrderByDescending(a => a.OccurredAt).Take(count).ToList();
    }

    // Employers
    public async Task<PagedResult<AdminEmployerListDto>> GetEmployersAsync(AdminEmployerQuery q, CancellationToken ct = default)
    {
        var query = _context.EmployerProfiles.AsQueryable();

        if (!string.IsNullOrEmpty(q.Keyword))
            query = query.Where(e => e.CompanyName.Contains(q.Keyword));

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(e => e.CreatedAtUtc)
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .Select(e => new AdminEmployerListDto
            {
                Id = e.Id,
                CompanyName = e.CompanyName,
                Email = e.Email,
                Phone = e.PhoneNumber,
                CreatedAtUtc = e.CreatedAtUtc,
                IsVerified = e.IsVerified,
                JobsPostedCount = e.JobPosts.Count
            })
            .ToListAsync(ct);

        return new PagedResult<AdminEmployerListDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = q.Page,
            PageSize = q.PageSize
        };
    }

    public async Task<AdminEmployerDetailDto?> GetEmployerDetailAsync(int id, CancellationToken ct = default)
    {
        var employer = await _context.EmployerProfiles
            .Include(e => e.JobPosts)
            .Include(e => e.Subscriptions)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken: ct);

        if (employer == null)
            return null;

        var recentJobs = employer.JobPosts
            .OrderByDescending(j => j.CreatedAtUtc)
            .Take(5)
            .Select(j => new AdminJobListDto
            {
                Id = j.Id,
                Title = j.Title,
                CompanyName = employer.CompanyName,
                CreatedAtUtc = j.CreatedAtUtc,
                Status = j.Status.ToString()
            })
            .ToList();

        return new AdminEmployerDetailDto
        {
            Id = employer.Id,
            CompanyName = employer.CompanyName,
            Industry = employer.Industry,
            Location = employer.Location,
            PhoneNumber = employer.PhoneNumber,
            Email = employer.Email,
            WebsiteUrl = employer.WebsiteUrl,
            Description = employer.Description,
            LogoPath = employer.LogoPath,
            IsVerified = employer.IsVerified,
            CreatedAtUtc = employer.CreatedAtUtc,
            RecentJobs = recentJobs
        };
    }

    public async Task<bool> SetUserActiveStatusAsync(string userId, bool isActive, string adminId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return false;

        user.IsActive = isActive;
        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            await LogActionAsync(adminId, "Admin", isActive ? "Activated" : "Deactivated", "Employers");
        }
        return result.Succeeded;
    }

    public async Task<bool> DeleteEmployerAsync(int id, string adminId)
    {
        var employer = await _context.EmployerProfiles.FirstOrDefaultAsync(e => e.Id == id);
        if (employer == null)
            return false;

        _context.EmployerProfiles.Remove(employer);
        await _context.SaveChangesAsync();
        await LogActionAsync(adminId, "Admin", "Deleted", "Employers", $"Employer: {employer.CompanyName}");
        return true;
    }

    // Candidates
    public async Task<PagedResult<AdminCandidateListDto>> GetCandidatesAsync(AdminCandidateQuery q, CancellationToken ct = default)
    {
        var query = _context.CandidateProfiles.AsQueryable();

        if (!string.IsNullOrEmpty(q.Keyword))
            query = query.Where(c => c.FullName.Contains(q.Keyword));

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(c => c.CreatedAtUtc)
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .Select(c => new AdminCandidateListDto
            {
                Id = c.Id,
                FullName = c.FullName,
                CreatedAtUtc = c.CreatedAtUtc,
                ResumeCount = c.Resumes.Count,
                UserId = c.UserId
            })
            .ToListAsync(ct);

        return new PagedResult<AdminCandidateListDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = q.Page,
            PageSize = q.PageSize
        };
    }

    public async Task<AdminCandidateDetailDto?> GetCandidateDetailAsync(int id, CancellationToken ct = default)
    {
        var candidate = await _context.CandidateProfiles
            .Include(c => c.Resumes)
            .Include(c => c.Applications)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken: ct);

        if (candidate == null)
            return null;

        var resumes = candidate.Resumes
            .Select(r => new AdminResumeListDto
            {
                Id = r.Id,
                Title = r.Title,
                CreatedAtUtc = r.CreatedAtUtc,
                IsDefault = r.IsDefault
            })
            .ToList();

        var applications = candidate.Applications
            .Select(a => new AdminApplicationListDto
            {
                Id = a.Id,
                JobTitle = a.JobPost?.Title ?? "Unknown",
                CandidateName = candidate.FullName,
                CompanyName = a.JobPost?.EmployerProfile?.CompanyName ?? "Unknown",
                Status = a.Status.ToString(),
                AppliedAtUtc = a.AppliedAtUtc
            })
            .ToList();

        return new AdminCandidateDetailDto
        {
            Id = candidate.Id,
            FullName = candidate.FullName,
            PhoneNumber = candidate.PhoneNumber,
            Location = candidate.Location,
            ProfessionalTitle = candidate.ProfessionalTitle,
            Summary = candidate.Summary,
            CreatedAtUtc = candidate.CreatedAtUtc,
            Resumes = resumes,
            Applications = applications
        };
    }

    public async Task<bool> DeleteCandidateAsync(int id, string adminId)
    {
        var candidate = await _context.CandidateProfiles.FirstOrDefaultAsync(c => c.Id == id);
        if (candidate == null)
            return false;

        _context.CandidateProfiles.Remove(candidate);
        await _context.SaveChangesAsync();
        await LogActionAsync(adminId, "Admin", "Deleted", "Candidates", $"Candidate: {candidate.FullName}");
        return true;
    }

    // Resume Templates
    public async Task<PagedResult<AdminResumeTemplateDto>> GetResumeTemplatesAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var query = _context.ResumeTemplates.AsQueryable();
        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(t => t.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new AdminResumeTemplateDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                Category = t.Category,
                PreviewImagePath = t.PreviewImagePath,
                IsActive = t.IsActive,
                IsPremium = t.IsPremium,
                Price = t.Price,
                DiscountPrice = t.DiscountPrice,
                TemplateType = t.TemplateType,
                CreatedAtUtc = t.CreatedAtUtc
            })
            .ToListAsync(ct);

        return new PagedResult<AdminResumeTemplateDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<AdminResumeTemplateDto?> GetResumeTemplateByIdAsync(int id, CancellationToken ct = default)
    {
        var template = await _context.ResumeTemplates
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken: ct);

        if (template == null)
            return null;

        return new AdminResumeTemplateDto
        {
            Id = template.Id,
            Name = template.Name,
            Description = template.Description,
            Category = template.Category,
            PreviewImagePath = template.PreviewImagePath,
            IsActive = template.IsActive,
            IsPremium = template.IsPremium,
            Price = template.Price,
            DiscountPrice = template.DiscountPrice,
            TemplateType = template.TemplateType,
            CreatedAtUtc = template.CreatedAtUtc
        };
    }

    public async Task<int> CreateResumeTemplateAsync(AdminResumeTemplateFormDto dto, string adminId)
    {
        var template = new ResumeTemplate
        {
            Name = dto.Name,
            Description = dto.Description,
            TemplateFilePath = dto.TemplateFilePath,
            Category = dto.Category,
            IsActive = dto.IsActive,
            IsPremium = dto.IsPremium,
            Price = dto.Price,
            DiscountPrice = dto.DiscountPrice,
            TemplateType = dto.TemplateType,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.ResumeTemplates.Add(template);
        await _context.SaveChangesAsync();
        await LogActionAsync(adminId, "Admin", "Created", "ResumeTemplates", $"Template: {template.Name}");
        return template.Id;
    }

    public async Task<bool> UpdateResumeTemplateAsync(int id, AdminResumeTemplateFormDto dto, string adminId)
    {
        var template = await _context.ResumeTemplates.FirstOrDefaultAsync(t => t.Id == id);
        if (template == null)
            return false;

        template.Name = dto.Name;
        template.Description = dto.Description;
        template.TemplateFilePath = dto.TemplateFilePath;
        template.Category = dto.Category;
        template.IsActive = dto.IsActive;
        template.IsPremium = dto.IsPremium;
        template.Price = dto.Price;
        template.DiscountPrice = dto.DiscountPrice;
        template.TemplateType = dto.TemplateType;

        _context.ResumeTemplates.Update(template);
        await _context.SaveChangesAsync();
        await LogActionAsync(adminId, "Admin", "Updated", "ResumeTemplates", $"Template: {template.Name}");
        return true;
    }

    public async Task<bool> DeleteResumeTemplateAsync(int id, string adminId)
    {
        var template = await _context.ResumeTemplates.FirstOrDefaultAsync(t => t.Id == id);
        if (template == null)
            return false;

        _context.ResumeTemplates.Remove(template);
        await _context.SaveChangesAsync();
        await LogActionAsync(adminId, "Admin", "Deleted", "ResumeTemplates", $"Template: {template.Name}");
        return true;
    }

    public async Task<bool> ToggleResumeTemplateStatusAsync(int id, string adminId)
    {
        var template = await _context.ResumeTemplates.FirstOrDefaultAsync(t => t.Id == id);
        if (template == null)
            return false;

        template.IsActive = !template.IsActive;
        _context.ResumeTemplates.Update(template);
        await _context.SaveChangesAsync();
        await LogActionAsync(adminId, "Admin", template.IsActive ? "Enabled" : "Disabled", "ResumeTemplates", $"Template: {template.Name}");
        return true;
    }

    // Jobs
    public async Task<PagedResult<AdminJobListDto>> GetJobsAsync(AdminJobQuery q, CancellationToken ct = default)
    {
        var query = _context.JobPosts.AsQueryable();

        if (!string.IsNullOrEmpty(q.Keyword))
            query = query.Where(j => j.Title.Contains(q.Keyword));

        if (!string.IsNullOrEmpty(q.Status) && Enum.TryParse<JobStatus>(q.Status, out var status))
            query = query.Where(j => j.Status == status);

        if (q.CategoryId.HasValue)
            query = query.Where(j => j.JobCategoryId == q.CategoryId);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(j => j.CreatedAtUtc)
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .Select(j => new AdminJobListDto
            {
                Id = j.Id,
                Title = j.Title,
                CompanyName = j.EmployerProfile!.CompanyName,
                CategoryName = j.JobCategory!.Name,
                CreatedAtUtc = j.CreatedAtUtc,
                Status = j.Status.ToString(),
                IsFeatured = j.IsFeatured,
                ApplicationsCount = j.Applications.Count
            })
            .ToListAsync(ct);

        return new PagedResult<AdminJobListDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = q.Page,
            PageSize = q.PageSize
        };
    }

    public async Task<bool> UpdateJobStatusAsync(int jobId, string newStatus, string adminId)
    {
        var job = await _context.JobPosts.FirstOrDefaultAsync(j => j.Id == jobId);
        if (job == null || !Enum.TryParse<JobStatus>(newStatus, out var status))
            return false;

        job.Status = status;
        _context.JobPosts.Update(job);
        await _context.SaveChangesAsync();
        await LogActionAsync(adminId, "Admin", "UpdatedStatus", "Jobs", $"Job: {job.Title}, Status: {newStatus}");
        return true;
    }

    public async Task<bool> ToggleJobFeaturedAsync(int jobId, string adminId)
    {
        var job = await _context.JobPosts.FirstOrDefaultAsync(j => j.Id == jobId);
        if (job == null)
            return false;

        job.IsFeatured = !job.IsFeatured;
        _context.JobPosts.Update(job);
        await _context.SaveChangesAsync();
        await LogActionAsync(adminId, "Admin", job.IsFeatured ? "Featured" : "Unfeatured", "Jobs", $"Job: {job.Title}");
        return true;
    }

    public async Task<bool> DeleteJobAsync(int jobId, string adminId)
    {
        var job = await _context.JobPosts.FirstOrDefaultAsync(j => j.Id == jobId);
        if (job == null)
            return false;

        _context.JobPosts.Remove(job);
        await _context.SaveChangesAsync();
        await LogActionAsync(adminId, "Admin", "Deleted", "Jobs", $"Job: {job.Title}");
        return true;
    }

    // Applications
    public async Task<PagedResult<AdminApplicationListDto>> GetApplicationsAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var query = _context.JobApplications.AsQueryable();
        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(a => a.AppliedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AdminApplicationListDto
            {
                Id = a.Id,
                JobTitle = a.JobPost!.Title,
                CandidateName = a.CandidateProfile!.FullName,
                CompanyName = a.JobPost.EmployerProfile!.CompanyName,
                Status = a.Status.ToString(),
                AppliedAtUtc = a.AppliedAtUtc
            })
            .ToListAsync(ct);

        return new PagedResult<AdminApplicationListDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // Subscription Plans
    public async Task<List<AdminSubscriptionPlanDto>> GetSubscriptionPlansAsync(CancellationToken ct = default)
    {
        return await _context.SubscriptionPlans
            .Select(p => new AdminSubscriptionPlanDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                DurationDays = p.DurationDays,
                JobPostLimit = p.JobPostLimit,
                CanFeatureJobs = p.CanFeatureJobs
            })
            .ToListAsync(ct);
    }

    public async Task<int> CreateSubscriptionPlanAsync(AdminSubscriptionPlanFormDto dto, string adminId)
    {
        var plan = new SubscriptionPlan
        {
            Name = dto.Name,
            Price = dto.Price,
            DurationDays = dto.DurationDays,
            JobPostLimit = dto.JobPostLimit,
            CanFeatureJobs = dto.CanFeatureJobs,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.SubscriptionPlans.Add(plan);
        await _context.SaveChangesAsync();
        await LogActionAsync(adminId, "Admin", "Created", "SubscriptionPlans", $"Plan: {plan.Name}");
        return plan.Id;
    }

    public async Task<bool> UpdateSubscriptionPlanAsync(int id, AdminSubscriptionPlanFormDto dto, string adminId)
    {
        var plan = await _context.SubscriptionPlans.FirstOrDefaultAsync(p => p.Id == id);
        if (plan == null)
            return false;

        plan.Name = dto.Name;
        plan.Price = dto.Price;
        plan.DurationDays = dto.DurationDays;
        plan.JobPostLimit = dto.JobPostLimit;
        plan.CanFeatureJobs = dto.CanFeatureJobs;

        _context.SubscriptionPlans.Update(plan);
        await _context.SaveChangesAsync();
        await LogActionAsync(adminId, "Admin", "Updated", "SubscriptionPlans", $"Plan: {plan.Name}");
        return true;
    }

    public async Task<bool> DeleteSubscriptionPlanAsync(int id, string adminId)
    {
        var plan = await _context.SubscriptionPlans.FirstOrDefaultAsync(p => p.Id == id);
        if (plan == null)
            return false;

        _context.SubscriptionPlans.Remove(plan);
        await _context.SaveChangesAsync();
        await LogActionAsync(adminId, "Admin", "Deleted", "SubscriptionPlans", $"Plan: {plan.Name}");
        return true;
    }

    // Payments
    public async Task<PagedResult<AdminPaymentListDto>> GetPaymentsAsync(AdminPaymentQuery q, CancellationToken ct = default)
    {
        var query = _context.PaymentTransactions.AsQueryable();
        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(p => p.TransactionDateUtc)
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .Select(p => new AdminPaymentListDto
            {
                Id = p.Id,
                Amount = p.Amount,
                Reference = p.Reference,
                TransactionDateUtc = p.TransactionDateUtc,
                Status = p.Status
            })
            .ToListAsync(ct);

        return new PagedResult<AdminPaymentListDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = q.Page,
            PageSize = q.PageSize
        };
    }

    public async Task<AdminPaymentStatsDto> GetPaymentStatsAsync(CancellationToken ct = default)
    {
        var totalRevenue = await _context.PaymentTransactions
            .Where(p => p.Status == "Completed")
            .SumAsync(p => p.Amount, cancellationToken: ct);

        var currentMonth = DateTime.UtcNow.Month;
        var currentYear = DateTime.UtcNow.Year;
        var monthlyRevenue = await _context.PaymentTransactions
            .Where(p => p.Status == "Completed" && p.TransactionDateUtc.Month == currentMonth && p.TransactionDateUtc.Year == currentYear)
            .SumAsync(p => p.Amount, cancellationToken: ct);

        var currentYear2 = DateTime.UtcNow.Year;
        var yearlyRevenue = await _context.PaymentTransactions
            .Where(p => p.Status == "Completed" && p.TransactionDateUtc.Year == currentYear2)
            .SumAsync(p => p.Amount, cancellationToken: ct);

        return new AdminPaymentStatsDto
        {
            TotalRevenue = totalRevenue,
            MonthlyRevenue = monthlyRevenue,
            YearlyRevenue = yearlyRevenue
        };
    }

    // CMS
    public async Task<List<AdminContentPageDto>> GetContentPagesAsync(CancellationToken ct = default)
    {
        return await _context.ContentPages
            .Select(p => new AdminContentPageDto
            {
                Key = p.Key,
                Title = p.Title,
                HtmlContent = p.HtmlContent,
                IsPublished = p.IsPublished
            })
            .ToListAsync(ct);
    }

    public async Task<AdminContentPageDto?> GetContentPageByKeyAsync(string key, CancellationToken ct = default)
    {
        var page = await _context.ContentPages
            .FirstOrDefaultAsync(p => p.Key == key, cancellationToken: ct);

        if (page == null)
            return null;

        return new AdminContentPageDto
        {
            Key = page.Key,
            Title = page.Title,
            HtmlContent = page.HtmlContent,
            IsPublished = page.IsPublished
        };
    }

    public async Task<bool> UpdateContentPageAsync(string key, string title, string html, string adminId)
    {
        var page = await _context.ContentPages.FirstOrDefaultAsync(p => p.Key == key);
        if (page == null)
            return false;

        page.Title = title;
        page.HtmlContent = html;

        _context.ContentPages.Update(page);
        await _context.SaveChangesAsync();
        await LogActionAsync(adminId, "Admin", "Updated", "ContentPages", $"Page: {key}");
        return true;
    }

    public async Task<PagedResult<AdminBlogListDto>> GetAllBlogsAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var query = _context.BlogPosts.AsQueryable();
        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(b => b.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new AdminBlogListDto
            {
                Id = b.Id,
                Title = b.Title,
                AuthorName = b.AuthorName,
                CategoryName = b.Category != null ? b.Category.Name : null,
                IsPublished = b.IsPublished,
                PublishedAtUtc = b.PublishedAtUtc,
                CreatedAtUtc = b.CreatedAtUtc
            })
            .ToListAsync(ct);

        return new PagedResult<AdminBlogListDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<bool> AdminDeleteBlogAsync(int blogId, string adminId)
    {
        var blog = await _context.BlogPosts.FirstOrDefaultAsync(b => b.Id == blogId);
        if (blog == null)
            return false;

        _context.BlogPosts.Remove(blog);
        await _context.SaveChangesAsync();
        await LogActionAsync(adminId, "Admin", "Deleted", "Blogs", $"Blog: {blog.Title}");
        return true;
    }

    public async Task<bool> AdminToggleBlogPublishAsync(int blogId, string adminId)
    {
        var blog = await _context.BlogPosts.FirstOrDefaultAsync(b => b.Id == blogId);
        if (blog == null)
            return false;

        blog.IsPublished = !blog.IsPublished;
        if (blog.IsPublished)
            blog.PublishedAtUtc = DateTime.UtcNow;

        _context.BlogPosts.Update(blog);
        await _context.SaveChangesAsync();
        await LogActionAsync(adminId, "Admin", blog.IsPublished ? "Published" : "Unpublished", "Blogs", $"Blog: {blog.Title}");
        return true;
    }

    // Reports
    public async Task<AdminReportDto> GetReportAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        var newEmployers = await _context.EmployerProfiles
            .CountAsync(e => e.CreatedAtUtc >= from && e.CreatedAtUtc <= to, cancellationToken: ct);

        var newCandidates = await _context.CandidateProfiles
            .CountAsync(c => c.CreatedAtUtc >= from && c.CreatedAtUtc <= to, cancellationToken: ct);

        var jobsPosted = await _context.JobPosts
            .CountAsync(j => j.CreatedAtUtc >= from && j.CreatedAtUtc <= to, cancellationToken: ct);

        var applicationsSubmitted = await _context.JobApplications
            .CountAsync(a => a.AppliedAtUtc >= from && a.AppliedAtUtc <= to, cancellationToken: ct);

        var subscriptionsPurchased = await _context.EmployerSubscriptions
            .CountAsync(s => s.StartsAtUtc >= from && s.StartsAtUtc <= to, cancellationToken: ct);

        var revenue = await _context.PaymentTransactions
            .Where(p => p.Status == "Completed" && p.TransactionDateUtc >= from && p.TransactionDateUtc <= to)
            .SumAsync(p => p.Amount, cancellationToken: ct);

        var resumes = await _context.CandidateResumes
            .CountAsync(r => r.CreatedAtUtc >= from && r.CreatedAtUtc <= to, cancellationToken: ct);

        return new AdminReportDto
        {
            FromDate = from,
            ToDate = to,
            NewEmployersRegistered = newEmployers,
            NewCandidatesRegistered = newCandidates,
            JobsPosted = jobsPosted,
            ApplicationsSubmitted = applicationsSubmitted,
            SubscriptionsPurchased = subscriptionsPurchased,
            TotalRevenueEarned = revenue,
            ResumesCreated = resumes
        };
    }

    // Audit Log
    public async Task<PagedResult<AdminAuditLogDto>> GetAuditLogsAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var query = _context.AdminAuditLogs.AsQueryable();
        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(a => a.PerformedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AdminAuditLogDto
            {
                Id = a.Id,
                AdminName = a.AdminName,
                Action = a.Action,
                Module = a.Module,
                Details = a.Details,
                PerformedAtUtc = a.PerformedAtUtc
            })
            .ToListAsync(ct);

        return new PagedResult<AdminAuditLogDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task LogActionAsync(string adminId, string adminName, string action, string module, string? details = null)
    {
        var log = new AdminAuditLog
        {
            AdminUserId = adminId,
            AdminName = adminName,
            Action = action,
            Module = module,
            Details = details,
            PerformedAtUtc = DateTime.UtcNow,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.AdminAuditLogs.Add(log);
        await _context.SaveChangesAsync();
    }
}
