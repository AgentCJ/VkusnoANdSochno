using Microsoft.Extensions.Logging;

namespace VkusnoAndSochno.App
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif
            builder.Services.AddSingleton<Services.ApiService>();
            builder.Services.AddSingleton<Services.UserSession>();
            builder.Services.AddTransient<ViewModels.CashierViewModel>();
            builder.Services.AddTransient<Views.CashierPage>();
            builder.Services.AddTransient<Views.LoginPage>();
            builder.Services.AddTransient<ViewModels.CookViewModel>();
            builder.Services.AddTransient<Views.CookPage>();
            builder.Services.AddTransient<ViewModels.StorekeeperViewModel>();
            builder.Services.AddTransient<Views.StorekeeperPage>();
            builder.Services.AddTransient<ViewModels.ManagerViewModel>();
            builder.Services.AddTransient<Views.ManagerPage>();
            builder.Services.AddTransient<Views.TestHubPage>(); //тестовый, удалить потом
            builder.Services.AddTransient<ViewModels.AdminViewModel>();
            builder.Services.AddTransient<Views.AdminPage>();

            return builder.Build();
        }
    }
}
