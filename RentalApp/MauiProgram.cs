using Microsoft.Extensions.Logging;
using RentalApp.ViewModels;
using RentalApp.Database.Data;
using RentalApp.Database.Data.Repositories;
using RentalApp.Views;
using RentalApp.Services;

namespace RentalApp;

public static class MauiProgram
{
    // Set to true to authenticate via the shared SET09102 REST API (JWT).
    // Set to false to use local PostgreSQL authentication (BCrypt).
    private const bool UseApiAuth = true;

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

        // --- Data access ---
        builder.Services.AddDbContext<AppDbContext>();

        // --- Repositories ---
        builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
        builder.Services.AddScoped<IItemRepository, ItemRepository>();
        builder.Services.AddScoped<IRentalRepository, RentalRepository>();
        builder.Services.AddScoped<IReviewRepository, ReviewRepository>();

        // --- Authentication (toggle via UseApiAuth) ---
        if (UseApiAuth)
        {
            builder.Services.AddSingleton(new HttpClient
            {
                BaseAddress = new Uri("https://set09102-api.b-davison.workers.dev")
            });
            builder.Services.AddSingleton<IAuthenticationService, ApiAuthenticationService>();
        }
        else
        {
            builder.Services.AddSingleton<IAuthenticationService, LocalAuthenticationService>();
        }

        // --- Services ---
        builder.Services.AddSingleton<INavigationService, NavigationService>();
        builder.Services.AddSingleton<ILocationService, LocationService>();
        builder.Services.AddTransient<IRentalService, RentalService>();

        // --- Shell ---
        builder.Services.AddSingleton<AppShellViewModel>();
        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddSingleton<App>();

        // --- Auth pages ---
        builder.Services.AddSingleton<LoginViewModel>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddSingleton<RegisterViewModel>();
        builder.Services.AddTransient<RegisterPage>();

        // --- Main dashboard ---
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<MainPage>();

        // --- Marketplace pages ---
        builder.Services.AddTransient<ItemsListViewModel>();
        builder.Services.AddTransient<ItemsListPage>();
        builder.Services.AddTransient<ItemDetailViewModel>();
        builder.Services.AddTransient<ItemDetailPage>();
        builder.Services.AddTransient<CreateItemViewModel>();
        builder.Services.AddTransient<CreateItemPage>();
        builder.Services.AddTransient<NearbyItemsViewModel>();
        builder.Services.AddTransient<NearbyItemsPage>();
        builder.Services.AddTransient<RentalsViewModel>();
        builder.Services.AddTransient<RentalsPage>();
        builder.Services.AddTransient<ReviewsViewModel>();
        builder.Services.AddTransient<ReviewsPage>();

        // --- Admin user management ---
        builder.Services.AddTransient<UserListViewModel>();
        builder.Services.AddTransient<UserListPage>();
        builder.Services.AddTransient<UserDetailPage>();
        builder.Services.AddTransient<UserDetailViewModel>();

        // --- Placeholder ---
        builder.Services.AddSingleton<TempViewModel>();
        builder.Services.AddTransient<TempPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
