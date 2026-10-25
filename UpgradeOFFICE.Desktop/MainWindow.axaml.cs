using System.Text;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace UpgradeOFFICE.Desktop;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        SheetEditor.Text = "Item,Quantidade,Preço\nCaderno,3,12.50\nCaneta,5,2.00";
    }

    private void SetStatus(string message) => StatusText.Text = message;

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
        try
        {
            var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = title,
                SuggestedFileName = suggestedName,
                FileTypeChoices = new[] { type }
            });

            if (file is null) return;

            await using var stream = await file.OpenWriteAsync();
            if (stream.CanSeek)
            {
                stream.SetLength(0);
                stream.Position = 0;
            }

            await using var writer = new StreamWriter(stream, new UTF8Encoding(false));
            await writer.WriteAsync(content);
            SetStatus($"Arquivo salvo: {file.Name}");
        }
        catch (Exception ex)
        {
            SetStatus($"Não foi possível salvar o arquivo: {ex.Message}");
        }
    }

    private static async Task<string> ReadTextAsync(IStorageFile file)
    {
        await using var stream = await file.OpenReadAsync();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }

    private static async Task<string> ReadWordDocumentAsync(IStorageFile file)
    {
        await using var stream = await file.OpenReadAsync();
        using var document = WordprocessingDocument.Open(stream, false);

        var body = document.MainDocumentPart?.Document?.Body;
        if (body is null) return string.Empty;

        // Mantém o texto e as quebras entre parágrafos. A formatação avançada
        // (tabelas, imagens, fontes, cores e estilos) ainda não é reproduzida no editor.
        return string.Join(
            Environment.NewLine,
            body.Descendants<Paragraph>().Select(paragraph =>
                string.Concat(paragraph.Descendants<Text>().Select(text => text.Text))));
    }

    private async Task SaveWordDocumentAsync(string content)
    {
        try
        {
            var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Salvar documento do Word",
                SuggestedFileName = "Documento.docx",
                FileTypeChoices = new[]
                {
                    new FilePickerFileType("Documento do Word") { Patterns = new[] { "*.docx" } }
                }
            });

            if (file is null) return;

            await using var stream = await file.OpenWriteAsync();
            if (stream.CanSeek)
            {
                stream.SetLength(0);
                stream.Position = 0;
            }

            using (var document = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document))
            {
                var mainPart = document.AddMainDocumentPart();
                mainPart.Document = new Document(new Body());
                var body = mainPart.Document.Body!;

                foreach (var line in content.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n'))
                {
                    var text = new Text(line) { Space = SpaceProcessingModeValues.Preserve };
                    body.AppendChild(new Paragraph(new Run(text)));
                }

                mainPart.Document.Save();
            }

            SetStatus($"Documento Word salvo: {file.Name}");
        }
        catch (Exception ex)
        {
            SetStatus($"Não foi possível salvar o documento Word: {ex.Message}");
        }
    }

    private async void OpenWriter_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        try
        {
            var file = await PickOpenFileAsync("Abrir documento", new FilePickerFileType("Arquivos de texto")
            {
                Patterns = new[] { "*.txt", "*.md", "*.html" }
            });

            if (file is null) return;
            WriterEditor.Text = await ReadTextAsync(file);
            SetStatus($"Documento aberto: {file.Name}");
        }
        catch (Exception ex)
        {
            SetStatus($"Não foi possível abrir o documento: {ex.Message}");
        }
    }

    private async void OpenWord_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        try
        {
            var file = await PickOpenFileAsync("Abrir documento do Word", new FilePickerFileType("Documentos do Word")
            {
                Patterns = new[] { "*.docx" }
            });

            if (file is null) return;
            WriterEditor.Text = await ReadWordDocumentAsync(file);
            SetStatus($"Documento Word aberto: {file.Name} — texto e parágrafos importados.");
        }
        catch (Exception ex)
        {
            SetStatus($"Não foi possível abrir o documento Word: {ex.Message}");
        }
    }

    private async void SaveWriter_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        await SaveTextAsync("Salvar documento de texto", "Documento.txt", WriterEditor.Text ?? string.Empty,
            new FilePickerFileType("Texto") { Patterns = new[] { "*.txt" } });
    }

    private async void SaveWord_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        await SaveWordDocumentAsync(WriterEditor.Text ?? string.Empty);
    }

    private async void OpenSheet_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        try
        {
            var file = await PickOpenFileAsync("Abrir planilha CSV", new FilePickerFileType("Arquivos CSV")
            {
                Patterns = new[] { "*.csv", "*.txt" }
            });

            if (file is null) return;
            SheetEditor.Text = await ReadTextAsync(file);
            SetStatus($"Planilha aberta: {file.Name}");
        }
        catch (Exception ex)
        {
            SetStatus($"Não foi possível abrir a planilha: {ex.Message}");
        }
    }

    private async void SaveSheet_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        await SaveTextAsync("Salvar planilha", "Planilha.csv", SheetEditor.Text ?? string.Empty,
            new FilePickerFileType("CSV") { Patterns = new[] { "*.csv" } });
    }

    private async void OpenSlides_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        try
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
            SetStatus($"Rascunho aberto: {file.Name}");
        }
        catch (Exception ex)
        {
            SetStatus($"Não foi possível abrir a apresentação: {ex.Message}");
        }
    }

    private async void SaveSlides_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var content = $"{SlideTitle.Text ?? string.Empty}{Environment.NewLine}{Environment.NewLine}{SlideBody.Text ?? string.Empty}";
        await SaveTextAsync("Salvar rascunho de apresentação", "Apresentacao.txt", content,
            new FilePickerFileType("Texto") { Patterns = new[] { "*.txt" } });
    }
}
