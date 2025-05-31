using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VXL_KPI_system.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

//This populates the database with departments and counselors from your whiteboards when the app first runs.

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (!context.Departments.Any())
    {
        context.Departments.AddRange(
            new Department { Name = "Admissions" },
            new Department { Name = "Vasa Consulting" }
        );
        context.SaveChanges();

        context.Counselors.AddRange(
            new Counselor { Name = "D.W.", DepartmentID = 1 },  // Admissions
            new Counselor { Name = "A.S.", DepartmentID = 1 },
            new Counselor { Name = "M.B.", DepartmentID = 2 },  // Vasa Consulting
            new Counselor { Name = "C.J.", DepartmentID = 2 }
        );
        context.SaveChanges();
    }
}

app.Run();
