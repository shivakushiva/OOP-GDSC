//READ ME:
// THE OOP PROJECT THAT I DEMONSTRATED IS FROM A COLLEAGUE
// HE ONLY ALLOWS TO USE THE FILES THAT ARE DEMONSTRATED THE VIDEO
// SO THERE WILL BE LOT OF SQUIGGLY LINES WHEN LAUNCHED ON VISUAL STUDIO
// THANKS FOR UNDERSTANDING

using Services.CalculatorServices;
using Services.UnitConverterServices;
using Views.Calculator;
using Views.Favorites;
using Views.Menu;
using Views.Menu.Age;
using Views.Menu.Age.Calendar;
using Views.Menu.BMI;
using Views.Menu.Discount;
using Views.Menu.Interest;
using Views.Menu.UnitConverters;
using Views.Menu.UnitsPopups;

namespace App.DI;

public class AppSpecificDIContainer : IAppSpecificDIContainer
{
    public void ConfigureAppSpecificServices(IServiceCollection services)
    {
        services.AddSingleton<Apps.App>()
            .AddSingleton<AppConfigService>()
            .AddSingleton<AppInitializeDataService>()
            .AddSingleton<AppResourceService>()
            .AddSingleton<AppThemeService>()
            .AddSingleton<BasicCalculatorService>()
            .AddSingleton<UnitConverterService>()

            .AddTransient<AgePageViewModel>()
            .AddTransient<AppearancePageViewModel>()
            .AddTransient<BodyMassIndexPageViewModel>()
            .AddTransient<CalendarPopupViewModel>()
            .AddTransient<DiscountPageViewModel>()
            .AddTransient<InterestCalculatorViewModel>()
            .AddTransient<LanguagePageViewModel>()
            .AddTransient<SimpleAlertViewModel>()
            .AddTransient<UnitConvertersPageViewModel>()
            .AddTransient<UnitsPopupViewModel>()

            .AddSingleton<CalculatorPage>()
            .AddSingleton<CalculatorPageViewModel>()
            .AddSingleton<FavoritesPage>()
            .AddSingleton<FavoritesPageViewModel>()
            .AddSingleton<MenuPage>()
            .AddSingleton<MenuPageViewModel>()
            .AddSingleton<SettingsPage>()
            .AddSingleton<SettingsPageViewModel>()

            .AddHttpClient();
    }
}