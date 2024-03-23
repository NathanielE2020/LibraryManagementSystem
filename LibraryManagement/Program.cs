using LibraryData;
using LibraryServices;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews(); 


            var configuration = builder.Configuration;
            var connectionString = configuration.GetConnectionString("LibraryConnection");

            builder.Services.AddMvc();
            builder.Services.AddSingleton(configuration);
            builder.Services.AddScoped<ILibraryAsset, LibraryAssetServices>();
            builder.Services.AddScoped<ICheckout, CheckoutServices>();
            builder.Services.AddScoped<IPatron, PatronServices>();
            builder.Services.AddScoped<ILibraryBranch, LibraryBranchServices>();

			builder.Services.AddDbContext<LibraryContext>(options =>
			{
				options.UseSqlServer(builder.Configuration["ConnectionStrings:LibraryConnection"],
					sqlServerOptions =>
					{
						sqlServerOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null);
					});
			});

			//builder.Services.AddDbContext<LibraryContext>(options
			//=> options.UseSqlServer(connectionString, b => b.MigrationsAssembly("LibraryManagement")));

			var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            //app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
