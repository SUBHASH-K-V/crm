using AcxiomCRM.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            string[] roles = { "Admin", "Manager", "SalesExecutive" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new ApplicationRole { Name = role, Description = role + " Role" });
                }
            }

            // Seed Admin
            var adminEmail = "admin@acxiomcrm.com";
            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Admin",
                    IsActive = true,
                    EmailConfirmed = true,
                    CreatedDate = DateTime.UtcNow
                };
                var result = await userManager.CreateAsync(admin, "Admin@123");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(admin, "Admin");
            }

            // Seed Manager
            var mgrEmail = "manager@acxiomcrm.com";
            var mgr = await userManager.FindByEmailAsync(mgrEmail);
            if (mgr == null)
            {
                mgr = new ApplicationUser
                {
                    UserName = mgrEmail,
                    Email = mgrEmail,
                    FullName = "Sales Manager",
                    IsActive = true,
                    EmailConfirmed = true,
                    CreatedDate = DateTime.UtcNow
                };
                var result = await userManager.CreateAsync(mgr, "Manager@123");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(mgr, "Manager");
            }

            // Seed Sales Executive
            var seEmail = "sales@acxiomcrm.com";
            var se = await userManager.FindByEmailAsync(seEmail);
            if (se == null)
            {
                se = new ApplicationUser
                {
                    UserName = seEmail,
                    Email = seEmail,
                    FullName = "Sales Executive",
                    IsActive = true,
                    EmailConfirmed = true,
                    CreatedDate = DateTime.UtcNow
                };
                var result = await userManager.CreateAsync(se, "Sales@123");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(se, "SalesExecutive");
            }

            // Seed Sample Customers if none exist
            if (!await context.Customers.AnyAsync())
            {
                var cust1 = new Customer
                {
                    CustomerCode = "CUST-00001",
                    CustomerName = "Tata Consultancy Services Ltd",
                    Email = "procurement@tcs.com",
                    Phone = "9820012345",
                    CompanyName = "TCS Group",
                    Address = "TCS House, Raveline Street, Fort",
                    City = "Mumbai",
                    State = "Maharashtra",
                    Status = "Active",
                    CreatedBy = admin!.Id,
                    OwnerId = se!.Id,
                    CreatedDate = DateTime.UtcNow.AddMonths(-3),
                    Notes = "Strategic enterprise client. Key focus on cloud modernization."
                };

                var cust2 = new Customer
                {
                    CustomerCode = "CUST-00002",
                    CustomerName = "Infosys Limited",
                    Email = "vendor@infosys.com",
                    Phone = "9845012345",
                    CompanyName = "Infosys",
                    Address = "Electronics City, Hosur Road",
                    City = "Bengaluru",
                    State = "Karnataka",
                    Status = "Active",
                    CreatedBy = admin.Id,
                    OwnerId = se.Id,
                    CreatedDate = DateTime.UtcNow.AddMonths(-2),
                    Notes = "Annual software subscription account."
                };

                var cust3 = new Customer
                {
                    CustomerCode = "CUST-00003",
                    CustomerName = "Wipro Enterprises",
                    Email = "solutions@wipro.com",
                    Phone = "9880012345",
                    CompanyName = "Wipro Ltd",
                    Address = "Doddakannelli, Sarjapur Road",
                    City = "Bengaluru",
                    State = "Karnataka",
                    Status = "Active",
                    CreatedBy = admin.Id,
                    OwnerId = mgr!.Id,
                    CreatedDate = DateTime.UtcNow.AddMonths(-1),
                    Notes = "Partner network account."
                };

                context.Customers.AddRange(cust1, cust2, cust3);
                await context.SaveChangesAsync();

                // Seed Leads
                var lead1 = new Lead
                {
                    LeadCode = "LEAD-00001",
                    LeadName = "Reliance Retail Division",
                    Email = "crm.lead@ril.com",
                    Phone = "9833012345",
                    CompanyName = "Reliance Industries",
                    Source = "Website",
                    Status = "Qualified",
                    Priority = "High",
                    ExpectedValue = 750000,
                    AssignedTo = se.Id,
                    CreatedDate = DateTime.UtcNow.AddDays(-20),
                    Notes = "Requested quotation for CRM licenses across 50 regional offices."
                };

                var lead2 = new Lead
                {
                    LeadCode = "LEAD-00002",
                    LeadName = "HDFC Bank Corporate Office",
                    Email = "techleads@hdfcbank.com",
                    Phone = "9821012345",
                    CompanyName = "HDFC Bank",
                    Source = "Referral",
                    Status = "Contacted",
                    Priority = "High",
                    ExpectedValue = 1200000,
                    AssignedTo = se.Id,
                    CreatedDate = DateTime.UtcNow.AddDays(-10),
                    Notes = "Initial demo completed. Follow-up meeting scheduled."
                };

                var lead3 = new Lead
                {
                    LeadCode = "LEAD-00003",
                    LeadName = "Zomato Technologies",
                    Email = "partnerships@zomato.com",
                    Phone = "9810012345",
                    CompanyName = "Zomato Ltd",
                    Source = "Cold Call",
                    Status = "New",
                    Priority = "Medium",
                    ExpectedValue = 350000,
                    AssignedTo = se.Id,
                    CreatedDate = DateTime.UtcNow.AddDays(-3),
                    Notes = "Cold outreach inquiry received via partner referral."
                };

                context.Leads.AddRange(lead1, lead2, lead3);
                await context.SaveChangesAsync();

                // Seed Opportunities
                var opp1 = new Opportunity
                {
                    OpportunityName = "TCS - Global Cloud CRM Rollout",
                    CustomerId = cust1.CustomerId,
                    Amount = 1500000,
                    Stage = "Negotiation",
                    Probability = 80,
                    ExpectedCloseDate = DateTime.Today.AddDays(15),
                    Status = "Open",
                    AssignedTo = se.Id,
                    CreatedDate = DateTime.UtcNow.AddDays(-40),
                    Notes = "Final commercial pricing negotiation in progress."
                };

                var opp2 = new Opportunity
                {
                    OpportunityName = "Infosys - Analytics Integration Pack",
                    CustomerId = cust2.CustomerId,
                    Amount = 600000,
                    Stage = "Proposal",
                    Probability = 50,
                    ExpectedCloseDate = DateTime.Today.AddDays(25),
                    Status = "Open",
                    AssignedTo = se.Id,
                    CreatedDate = DateTime.UtcNow.AddDays(-25),
                    Notes = "Proposal submitted; waiting for executive sign-off."
                };

                var opp3 = new Opportunity
                {
                    OpportunityName = "Wipro - Annual Support & Maintenance",
                    CustomerId = cust3.CustomerId,
                    Amount = 450000,
                    Stage = "Won",
                    Probability = 100,
                    ExpectedCloseDate = DateTime.Today.AddDays(-5),
                    Status = "Closed",
                    AssignedTo = mgr.Id,
                    CreatedDate = DateTime.UtcNow.AddDays(-60),
                    Notes = "Deal closed and contract signed."
                };

                var opp4 = new Opportunity
                {
                    OpportunityName = "Reliance Retail CRM Pilot",
                    LeadId = lead1.LeadId,
                    CustomerId = cust1.CustomerId,
                    Amount = 850000,
                    Stage = "Qualification",
                    Probability = 30,
                    ExpectedCloseDate = DateTime.Today.AddDays(45),
                    Status = "Open",
                    AssignedTo = se.Id,
                    CreatedDate = DateTime.UtcNow.AddDays(-5),
                    Notes = "Initial scope document shared with lead."
                };

                context.Opportunities.AddRange(opp1, opp2, opp3, opp4);
                await context.SaveChangesAsync();

                // Seed Follow-ups
                var fu1 = new FollowUp
                {
                    CustomerId = cust1.CustomerId,
                    OpportunityId = opp1.OpportunityId,
                    FollowUpDate = DateTime.Today.AddDays(2),
                    Subject = "Negotiate licensing discounts with TCS VP",
                    FollowUpType = "Meeting",
                    Status = "Planned",
                    AssignedTo = se.Id,
                    Remarks = "Prepare SLA appendix and discounted tier options."
                };

                var fu2 = new FollowUp
                {
                    LeadId = lead2.LeadId,
                    FollowUpDate = DateTime.Today.AddDays(-2),
                    Subject = "Call HDFC Bank Tech Lead for feedback",
                    FollowUpType = "Call",
                    Status = "Planned",
                    AssignedTo = se.Id,
                    Remarks = "Follow up regarding security compliance assessment (overdue)."
                };

                var fu3 = new FollowUp
                {
                    CustomerId = cust2.CustomerId,
                    FollowUpDate = DateTime.Today.AddDays(-10),
                    Subject = "Completed QBR with Infosys Account Manager",
                    FollowUpType = "Meeting",
                    Status = "Completed",
                    AssignedTo = se.Id,
                    Remarks = "Client satisfied with current deliverables."
                };

                context.FollowUps.AddRange(fu1, fu2, fu3);

                // Seed Activities
                var act1 = new Activity
                {
                    ActivityType = "Meeting",
                    Subject = "Solution Architecture Presentation",
                    Description = "Presented multi-tenant architecture to enterprise technical committee.",
                    ActivityDate = DateTime.Today.AddDays(-12),
                    CustomerId = cust1.CustomerId,
                    OpportunityId = opp1.OpportunityId,
                    AssignedTo = se.Id,
                    Status = "Completed"
                };

                var act2 = new Activity
                {
                    ActivityType = "Call",
                    Subject = "Discovery call with VP of Engineering",
                    Description = "Understood business pain points with legacy CRM system.",
                    ActivityDate = DateTime.Today.AddDays(-7),
                    LeadId = lead1.LeadId,
                    AssignedTo = se.Id,
                    Status = "Completed"
                };

                context.Activities.AddRange(act1, act2);

                // Seed Audit Logs
                var audit1 = new AuditLog
                {
                    UserId = admin.Id,
                    Action = "SystemInit",
                    EntityName = "Database",
                    RecordId = "0",
                    NewValue = "Database initialized and seeded",
                    CreatedDate = DateTime.UtcNow.AddMonths(-3),
                    IpAddress = "127.0.0.1"
                };

                var audit2 = new AuditLog
                {
                    UserId = admin.Id,
                    Action = "Create",
                    EntityName = "Customer",
                    RecordId = "1",
                    NewValue = "Tata Consultancy Services Ltd",
                    CreatedDate = DateTime.UtcNow.AddMonths(-3),
                    IpAddress = "127.0.0.1"
                };

                context.AuditLogs.AddRange(audit1, audit2);
                await context.SaveChangesAsync();
            }
        }
    }
}
