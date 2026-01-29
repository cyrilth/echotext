using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using EchoText.Views;
using EchoText.ViewModels;
using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace EchoText;

public partial class App : Application
{
    private readonly IServiceProvider? _serviceProvider;

    public App()
    {
    }

    public App(IServiceProvider? serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit.
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();

            // Get MainViewModel from DI container
            var mainViewModel = _serviceProvider?.GetService<MainViewModel>();
            if (mainViewModel == null)
            {
                throw new InvalidOperationException("MainViewModel could not be resolved from DI container");
            }

            desktop.MainWindow = new MainWindow(mainViewModel);
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}