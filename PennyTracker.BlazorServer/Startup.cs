using System;
using System.Collections.Generic;

using BlazorDateRangePicker;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using PennyTracker.BlazorServer.Services;
using PennyTracker.BlazorServer.ViewModels;

using Prism.Events;

using Radzen;

namespace PennyTracker.BlazorServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDateRangePicker(config =>
            {
                config.Attributes = new Dictionary<string, object>
                {
                    { "class", "form-control form-control-sm" }
                };
            });

            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddHttpClient<IExpenseService, ExpenseService>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:44326/");
            });

            

            services.AddScoped<ApplicationState>();
            services.AddScoped<DialogService>();
            services.AddScoped<IDialogService, AppDialogService>();
            services.AddScoped<IEventAggregator, EventAggregator>();

            services.AddScoped<NotificationService>();
            services.AddScoped<ITransactionsComponentViewModel, TransactionsComponentViewModel>();
            services.AddScoped<ICreateExpenseComponentViewModel, CreateExpenseComponentViewModel>();
            services.AddScoped<IReportPageViewModel, ReportPageViewModel>();
            services.AddScoped<IPeriodComponentViewModel, PeriodComponentViewModel>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAuthentication();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
