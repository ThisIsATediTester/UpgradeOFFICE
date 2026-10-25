using System.Text;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace UpgradeOFFICE.Desktop;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        SheetEditor.Text = "Item,Quantidade,Preço\nCaderno,3,12.50\nCaneta,5,2.00";
    }

    private async Task<IStorageFile?> PickOpenFileAsync(string title, params FilePickerFileType[] types)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = false,
            FileTypeFilter = types.Length == 0 ? null : types
        });
        return files.Count > 0 ? files[0] : null;
    }

    private async Task SaveTextAsync(string title, string suggestedName, string content, FilePickerFileType type)
    {
        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = title,
            SuggestedFileName = suggestedName,
            FileTypeChoices = new[] { type }
        });

        if (file is null) return;

        await using var stream = await file.OpenWriteAsync();
        stream.SetLength(0);
        await using var writer = new StreamWriter(stream, new UTF8Encoding(false));
        await writer.WriteAsync(content);
    }

    private static async Task<string> ReadTextAsync(IStorageFile file)
    {
        await using var stream = await file.OpenReadAsync();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }

    private async void OpenWriter_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var file = await PickOpenFileAsync("Abrir documento", new FilePickerFileType("Documentos de texto")
        {
            Patterns = new[] { "*.txt", "*.md", "*.html" }
        });
        if (file is not null) WriterEditor.Text = await ReadTextAsync(file);
    }

    private async void SaveWriter_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        await SaveTextAsync("Salvar documento", "Documento.txt", WriterEditor.Text ?? string.Empty,
            new FilePickerFileType("Texto") { Patterns = new[] { "*.txt" } });
    }

    private async void OpenSheet_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var file = await PickOpenFileAsync("Abrir planilha CSV", new FilePickerFileType("Arquivos CSV")
        {
            Patterns = new[] { "*.csv", "*.txt" }
        });
        if (file is not null) SheetEditor.Text = await ReadTextAsync(file);
    }

    private async void SaveSheet_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        await SaveTextAsync("Salvar planilha", "Planilha.csv", SheetEditor.Text ?? string.Empty,
            new FilePickerFileType("CSV") { Patterns = new[] { "*.csv" } });
    }

    private async void OpenSlides_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var file = await PickOpenFileAsync("Abrir rascunho de apresentação", new FilePickerFileType("Arquivos de texto")
        {
            Patterns = new[] { "*.txt", "*.md" }
        });
        if (file is null) return;

        var content = await ReadTextAsync(file);
        var separator = content.IndexOf('\n');
        SlideTitle.Text = separator >= 0 ? content[..separator].TrimEnd('\r') : content;
        SlideBody.Text = separator >= 0 ? content[(separator + 1)..].TrimStart('\r', '\n') : string.Empty;
    }

    private async void SaveSlides_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var content = $"{SlideTitle.Text ?? string.Empty}{Environment.NewLine}{Environment.NewLine}{SlideBody.Text ?? string.Empty}";
        await SaveTextAsync("Salvar rascunho", "Apresentacao.txt", content,
            new FilePickerFileType("Texto") { Patterns = new[] { "*.txt" } });
    }
}
