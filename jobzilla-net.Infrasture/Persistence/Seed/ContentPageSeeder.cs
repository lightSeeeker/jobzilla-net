using jobzilla_net.Core.Entities;
using jobzilla_net.Infrasture.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace jobzilla_net.Infrasture.Seed;

public static class ContentPageSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Skip if pages already exist
        if (await context.ContentPages.AsQueryable().AnyAsync())
            return;

        var pages = new List<ContentPage>
        {
            new()
            {
                Key = "about",
                Title = "About Us",
                HtmlContent = @"
                    <h1>About Jobzilla</h1>
                    <p>Jobzilla is a comprehensive job portal platform that connects employers with qualified candidates.</p>
                    <h2>Our Mission</h2>
                    <p>We aim to revolutionize the job search and hiring process by providing a user-friendly platform that brings together the best talent with the best opportunities.</p>
                    <h2>Our Vision</h2>
                    <p>To be the leading job portal platform trusted by millions of professionals and employers worldwide.</p>
                ",
                IsPublished = true,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            },
            new()
            {
                Key = "privacy-policy",
                Title = "Privacy Policy",
                HtmlContent = @"
                    <h1>Privacy Policy</h1>
                    <p>At Jobzilla, we are committed to protecting your privacy. This Privacy Policy explains how we collect, use, disclose, and otherwise handle your information.</p>
                    <h2>Information We Collect</h2>
                    <p>We collect information you voluntarily provide when you create an account, apply for jobs, post jobs, or contact us.</p>
                    <h2>How We Use Your Information</h2>
                    <p>We use the information we collect to provide, maintain, and improve our services, process transactions, and comply with legal obligations.</p>
                    <h2>Your Rights</h2>
                    <p>You have the right to access, update, or delete your personal information at any time by contacting us.</p>
                ",
                IsPublished = true,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            },
            new()
            {
                Key = "terms-of-service",
                Title = "Terms of Service",
                HtmlContent = @"
                    <h1>Terms of Service</h1>
                    <p>By using Jobzilla, you agree to comply with these Terms of Service.</p>
                    <h2>User Responsibilities</h2>
                    <p>You are responsible for maintaining the confidentiality of your account information and password. You agree to accept responsibility for all activities that occur under your account.</p>
                    <h2>Prohibited Conduct</h2>
                    <p>You agree not to use Jobzilla for unlawful purposes or in any way that could damage, disable, or impair the platform.</p>
                    <h2>Limitation of Liability</h2>
                    <p>Jobzilla is provided on an 'as-is' basis. We make no warranties regarding the accuracy or reliability of the information provided on our platform.</p>
                ",
                IsPublished = true,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            },
            new()
            {
                Key = "faq",
                Title = "Frequently Asked Questions",
                HtmlContent = @"
                    <h1>Frequently Asked Questions</h1>
                    <h2>How do I create an account?</h2>
                    <p>Click on the Sign Up button on the homepage and follow the registration process. You can sign up as either a job seeker or employer.</p>
                    <h2>How do I search for jobs?</h2>
                    <p>Use the search bar on the Jobs page to search by keyword, location, or category. You can also apply filters to narrow down your results.</p>
                    <h2>Is there a fee to post jobs?</h2>
                    <p>Yes, job posting requires a subscription plan. We offer various plans to suit different needs and budgets.</p>
                    <h2>How do I contact support?</h2>
                    <p>You can reach our support team through the Contact Us page or by emailing support@jobzilla.com.</p>
                ",
                IsPublished = true,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            },
            new()
            {
                Key = "contact",
                Title = "Contact Us",
                HtmlContent = @"
                    <h1>Contact Us</h1>
                    <p>We'd love to hear from you. Get in touch with us for any inquiries or feedback.</p>
                    <h2>Email</h2>
                    <p><strong>Email:</strong> support@jobzilla.com</p>
                    <h2>Phone</h2>
                    <p><strong>Phone:</strong> +1 (555) 123-4567</p>
                    <h2>Office Address</h2>
                    <p>
                        Jobzilla Headquarters<br/>
                        123 Business Street<br/>
                        New York, NY 10001<br/>
                        United States
                    </p>
                    <h2>Business Hours</h2>
                    <p>Monday - Friday: 9:00 AM - 6:00 PM EST</p>
                ",
                IsPublished = true,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            }
        };

        await context.ContentPages.AddRangeAsync(pages);
        await context.SaveChangesAsync();
    }
}
