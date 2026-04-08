using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GestionPro.Data;
using GestionPro.Services.Interfaces;
using GestionPro.Services.Implementations;

var builder = WebApplication.CreateBuilder(args);

// ── EF Core + SQLite ──
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Identity con Roles ──
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<AppDbContext>();

// ── Cookie de autenticación ──
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
});

// ── MVC + Razor Pages ──
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// ── AutoMapper ──
builder.Services.AddAutoMapper(typeof(Program));

// ── HttpContextAccessor (para auditoría automática en DbContext) ──
builder.Services.AddHttpContextAccessor();

// ── Services (Inyección de Dependencias) ──
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<IOrdenService, OrdenService>();
builder.Services.AddScoped<IFacturaService, FacturaService>();

var app = builder.Build();

// ── Seed Data ──
await SeedData.InicializarAsync(app.Services);

// ── Middleware Pipeline ──
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
