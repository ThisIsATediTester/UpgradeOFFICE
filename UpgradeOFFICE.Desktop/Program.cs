using Avalonia;

namespace UpgradeOFFICE.Desktop;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args) =>
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .With(new Win32PlatformOptions
            {
                // Use CPU rendering on Windows to avoid native GPU/ANGLE crashes
                // on machines with incompatible graphics drivers.
                RenderingMode = new[] { Win32RenderingMode.Software },
                CompositionMode = new[] { Win32CompositionMode.RedirectionSurface }
            })
            .WithInterFont()
            .LogToTrace();
}
