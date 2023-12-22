using BTS_v3.Hubs;
using BTS_v3.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
var provider = builder.Services.BuildServiceProvider();
var config = provider.GetRequiredService<IConfiguration>();

//�Խÿ�
builder.Services.AddDbContext<AppDbContext>(item => item.UseSqlServer(config.GetConnectionString("WMAHUData")));

//���ÿ� + appsettings/json
//builder.Services.AddDbContext<AppDbContext>(item => item.UseSqlServer(config.GetConnectionString("DefaultConnection")));

//ê�� ��
builder.Services.AddHttpClient();

//SingalR
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=login}/{id?}");

//signalR
app.MapHub<DataHub>("/datahub");
app.MapHub<ChartHub>("/charthub");


app.Run();
