#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chummer.Annotations;
using ChummerRazorLibrary;
using ChummerRazorLibrary.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Chummer.Blazor;

public static class BlazorUtility
{
    private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
    private static Logger Log => s_ObjLogger.Value;


    private static IServiceProvider? _defaultServiceProvider;

    /// <summary>
    /// A <see cref="IServiceProvider"/> that contains a default list of services needed to spawn a <see cref="BlazorWebView"/>.
    /// It is cached and thus will only be build once and then reused among all other following components.
    /// </summary>
    private static IServiceProvider DefaultServiceProvider
    {
        get
        {
            if (_defaultServiceProvider is not null)
            {
                return _defaultServiceProvider;
            }

            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            var services = new ServiceCollection();
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddNLog(config);
            });

            services.AddChummerServices();
            services.AddChummerRazorLibraryServices();
            services.AddWindowsFormsBlazorWebView();
#if DEBUG
            services.AddBlazorWebViewDeveloperTools();
#endif
            _defaultServiceProvider = services.BuildServiceProvider();
            return _defaultServiceProvider;
        }
    }

    /// <summary>
    /// Define all services that may be needed for Blazor components here.
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <returns></returns>
    private static IServiceCollection AddChummerServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<IAboutViewModel, AboutViewModel>();

        return serviceCollection;
    }


    /// <summary>
    /// Convenience Method to configure a <see cref="BlazorWebView"/> with all the needed services.
    /// </summary>
    /// <param name="blazorWebView" />
    /// <param name="parameters" />
    public static BlazorWebView ConfigureBlazorWebView<TComponent>(this BlazorWebView blazorWebView, IDictionary<string, object?>? parameters = null)
        where TComponent : IComponent
    {
        if (!IsWebViewRuntimeInstalled)
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var browserExecutableFolder = Path.Combine(baseDirectory, "WebView2Runtime");
            Environment.SetEnvironmentVariable("WEBVIEW2_BROWSER_EXECUTABLE_FOLDER", browserExecutableFolder);
        }


        parameters ??= new Dictionary<string, object?>();
        blazorWebView.HostPage = "wwwroot\\index.html";
        blazorWebView.Services = DefaultServiceProvider;

        var wrapperParams = new Dictionary<string, object?>()
        {
            {nameof(SharedWrapper.IsLightMode), ColorManager.IsLightMode}
        };

        blazorWebView.RootComponents.Add<SharedWrapper>("#app-wrapper", wrapperParams);
        blazorWebView.RootComponents.Add<TComponent>("#app", parameters);

        return blazorWebView;
    }

    private static bool _isWebViewRuntimeInstalled;
    private static bool _runtimeChecked;
    private static bool IsWebViewRuntimeInstalled
    {
        get
        {
            if (_runtimeChecked)
            {
                return _isWebViewRuntimeInstalled;
            }

            _isWebViewRuntimeInstalled = CheckWebView2Runtime();
            return _isWebViewRuntimeInstalled;
        }
    }


    private static string GetVersionFromRegistry(string path)
    {
        var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(path);
        if (key != null)
        {
            var version = key.GetValue("pv") as string;
            if (!string.IsNullOrEmpty(version) && string.Compare(version, "0.0.0.0", StringComparison.OrdinalIgnoreCase) > 0)
                return version;
        }

        key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(path);
        if (key != null)
        {
            var version = key.GetValue("pv") as string;
            if (!string.IsNullOrEmpty(version) && string.Compare(version, "0.0.0.0", StringComparison.OrdinalIgnoreCase) > 0)
                return version;
        }

        return null;
    }

    private static bool CheckWebView2Runtime()
    {
        _runtimeChecked = true;
        string[] keys = new string[]
        {
            @"SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}",
            @"SOFTWARE\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}"
        };

        return keys.Select(GetVersionFromRegistry).Any(version => !string.IsNullOrEmpty(version));
    }

}

