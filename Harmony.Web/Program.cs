using Harmony.Application.Features.People.Queries.GetPeople;
using Harmony.Infrastructure.Persistence;
using Harmony.Web.Components;
using Microsoft.EntityFrameworkCore;
using Blazorise;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddBlazorise(options =>
    {
        options.Immediate = true; // Optional: Enable immediate rendering
    })
    .AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddValidatorsFromAssemblyContaining(typeof(GetPeopleQuery));

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(GetPeopleQuery).Assembly);
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

Harmony.Infrastructure.DependencyInjection.AddInfrastructure(builder.Services, builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();