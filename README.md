# EZMS.UI
This is the User interface part of the EZms CMS platform

To enable the UI you need to add the following to your .NET Core startup

_**Note:** you will need to have added the [EZms.Core](https://github.com/Floydan/EZms.Core/) middleware and requirements to the startup as well_


```csharp

private readonly CultureInfo[] _supportedCultures = {
    new CultureInfo("sv"),
    new CultureInfo("en")
};

public void ConfigureServices(IServiceCollection services) {
...
    //Example of localization setup, this can be any type of localization as long as it follows the standard localization flow.
    services.AddTransient<IStringLocalizerFactory, JsonStringLocalizerFactory>();
            services.AddTransient<IStringLocalizer, JsonStringLocalizer>();
            services.AddLocalization(options => options.ResourcesPath = $@"{HostingEnvironment.ContentRootPath}\Resources");

    //Request localization 
    services.Configure<RequestLocalizationOptions>(options =>
    {
        options.DefaultRequestCulture = new RequestCulture(culture: DefaultCulture, uiCulture: DefaultCulture);
        options.SupportedCultures = _supportedCultures;
        options.SupportedUICultures = _supportedCultures;
    });

    // ========================================================
    // Add the requirements for DB and EZms.Core middleware here
    // ========================================================

    //This is needed by several parts of the UI
    services.AddSingleton(HostingEnvironment);

    //Authorization
    services.AddTransient<UserManager<IdentityUser>>();

    //EZms automapper, UI and ServiceLocator middleware
    services.SetupEZmsAutomapper()
            .AddEZmsUI()
            .SetupEZmsServiceLocator();

    //Razor page authorization, view localization and data annotation localization
    services.AddRazorPagesOptions(options =>
    {
        options.Conventions.AuthorizePage("/ezms");
        options.Conventions.AuthorizeAreaFolder("EZms", "/EZms");
    })
    .AddViewLocalization()
    .AddDataAnnotationsLocalization()
    .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
...
}

public void Configure(IApplicationBuilder app, IHostingEnvironment env) {

    app.UseRequestLocalization(new RequestLocalizationOptions {
        DefaultRequestCulture = new RequestCulture(DefaultCulture),
        // Formatting numbers, dates, etc.
        SupportedCultures = _supportedCultures,
        // UI strings that we have localized.
        SupportedUICultures = _supportedCultures
    });

    var locOptions = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
    app.UseRequestLocalization(locOptions.Value);
    
    app.UseEZmsAzureFilesProvider(env);
    app.UseAuthentication();

    app.UseStatusCodePages(async context =>
    {
        var response = context.HttpContext.Response;

        //If the requested url contains /ezms you can redirect the user to the ezms specific error page here

        if (response.StatusCode == (int)HttpStatusCode.Unauthorized ||
            response.StatusCode == (int)HttpStatusCode.Forbidden)
            response.Redirect("/EZmsIdentity/Account/AccessDenied");
    });
}
```