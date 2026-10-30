using System.Globalization;
using System.Text;
using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaTextAlignment = Avalonia.Media.TextAlignment;
using Avalonia.Platform.Storage;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace UpgradeOFFICE.Desktop;

public partial class MainWindow : Window
{
    private sealed class WriterParagraph
    {
        public required TextBox Editor { get; init; }
        public bool IsBold { get; set; }
        public bool IsItalic { get; set; }
        public bool IsUnderline { get; set; }
        public int FontSize { get; set; } = 12;
        public AvaloniaTextAlignment Alignment { get; set; } = AvaloniaTextAlignment.Left;
    }

    private readonly List<WriterParagraph> _writerParagraphs = new();
    private WriterParagraph? _activeParagraph;
    private bool _loadingWriter;

    public MainWindow()
    {
        InitializeComponent();
        SheetEditor.Text = "Item,Quantidade,Preço\nCaderno,3,12.50\nCaneta,5,2.00";
        SetWriterText(string.Empty);
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

    private void SetWriterText(string content)
    {
        _loadingWriter = true;
        try
        {
            WriterParagraphs.Children.Clear();
            _writerParagraphs.Clear();
            _activeParagraph = null;

            var normalized = content.Replace("\r\n", "\n").Replace('\r', '\n');
            var lines = normalized.Split('\n');
            if (lines.Length == 0) lines = new[] { string.Empty };

            foreach (var line in lines)
                AddWriterParagraph(line, false);

            if (_writerParagraphs.Count > 0)
                _activeParagraph = _writerParagraphs[0];
        }
        finally
        {
            _loadingWriter = false;
        }
    }

    private WriterParagraph AddWriterParagraph(string text, bool focus)
    {
        var editor = new TextBox
        {
            Text = text,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            FontSize = 12,
            MinHeight = 34,
            VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center,
            HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            Background = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.Parse("#E5EAF1")),
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(8, 6),
            Watermark = _writerParagraphs.Count == 0 ? "Comece a escrever aqui..." : null
        };

        var paragraph = new WriterParagraph { Editor = editor };
        editor.GotFocus += (_, _) =>
        {
            _activeParagraph = paragraph;
            SyncToolbarFromActiveParagraph();
        };

        WriterParagraphs.Children.Add(editor);
        _writerParagraphs.Add(paragraph);
        ApplyParagraphVisualStyle(paragraph);

        if (focus)
        {
            _activeParagraph = paragraph;
            editor.Focus();
            editor.CaretIndex = editor.Text?.Length ?? 0;
        }

        return paragraph;
    }

    private void ApplyParagraphVisualStyle(WriterParagraph paragraph)
    {
        paragraph.Editor.FontWeight = paragraph.IsBold ? FontWeight.Bold : FontWeight.Normal;
        paragraph.Editor.FontStyle = paragraph.IsItalic ? FontStyle.Italic : FontStyle.Normal;
        paragraph.Editor.FontSize = paragraph.FontSize;
        paragraph.Editor.AvaloniaTextAlignment = paragraph.Alignment;
        // Avalonia's standard TextBox has no inline rich-text runs; underline is
        // represented in the document model and written to DOCX on save.
    }

    private string GetWriterText() =>
        string.Join(Environment.NewLine, _writerParagraphs.Select(p => p.Editor.Text ?? string.Empty));

    private WriterParagraph? GetActiveParagraph()
    {
        if (_activeParagraph is not null && _writerParagraphs.Contains(_activeParagraph))
            return _activeParagraph;

        _activeParagraph = _writerParagraphs.FirstOrDefault();
        return _activeParagraph;
    }

    private void SyncToolbarFromActiveParagraph()
    {
        var paragraph = GetActiveParagraph();
        if (paragraph is null || WriterFontSize is null) return;

        var wanted = paragraph.FontSize.ToString(CultureInfo.InvariantCulture);
        WriterFontSize.SelectedItem = WriterFontSize.Items
            .OfType<ComboBoxItem>()
            .FirstOrDefault(item => string.Equals(item.Content?.ToString(), wanted, StringComparison.Ordinal));
    }

    private void ToggleBold_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var paragraph = GetActiveParagraph();
        if (paragraph is null) return;
        paragraph.IsBold = !paragraph.IsBold;
        ApplyParagraphVisualStyle(paragraph);
        paragraph.Editor.Focus();
        SetStatus($"Negrito {(paragraph.IsBold ? "ativado" : "desativado")} no parágrafo atual.");
    }

    private void ToggleItalic_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var paragraph = GetActiveParagraph();
        if (paragraph is null) return;
        paragraph.IsItalic = !paragraph.IsItalic;
        ApplyParagraphVisualStyle(paragraph);
        paragraph.Editor.Focus();
        SetStatus($"Itálico {(paragraph.IsItalic ? "ativado" : "desativado")} no parágrafo atual.");
    }

    private void ToggleUnderline_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var paragraph = GetActiveParagraph();
        if (paragraph is null) return;
        paragraph.IsUnderline = !paragraph.IsUnderline;
        paragraph.Editor.Focus();
        SetStatus($"Sublinhado {(paragraph.IsUnderline ? "ativado" : "desativado")}; será aplicado ao salvar em Word.");
    }

    private void WriterFontSize_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_loadingWriter || WriterFontSize.SelectedItem is not ComboBoxItem item ||
            !int.TryParse(item.Content?.ToString(), out var size))
            return;

        var paragraph = GetActiveParagraph();
        if (paragraph is null) return;
        paragraph.FontSize = size;
        ApplyParagraphVisualStyle(paragraph);
        SetStatus($"Tamanho {size} aplicado ao parágrafo atual.");
    }

    private void SetParagraphAlignment(AvaloniaTextAlignment alignment, string label)
    {
        var paragraph = GetActiveParagraph();
        if (paragraph is null) return;
        paragraph.Alignment = alignment;
        ApplyParagraphVisualStyle(paragraph);
        paragraph.Editor.Focus();
        SetStatus($"Alinhamento {label} aplicado ao parágrafo atual.");
    }

    private void AlignLeft_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        SetParagraphAlignment(AvaloniaTextAlignment.Left, "à esquerda");

    private void AlignCenter_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        SetParagraphAlignment(AvaloniaTextAlignment.Center, "centralizado");

    private void AlignRight_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        SetParagraphAlignment(AvaloniaTextAlignment.Right, "à direita");

    private void AddParagraph_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var previous = GetActiveParagraph();
        var paragraph = AddWriterParagraph(string.Empty, true);
        if (previous is not null)
        {
            paragraph.IsBold = previous.IsBold;
            paragraph.IsItalic = previous.IsItalic;
            paragraph.IsUnderline = previous.IsUnderline;
            paragraph.FontSize = previous.FontSize;
            paragraph.Alignment = previous.Alignment;
        }

        ApplyParagraphVisualStyle(paragraph);
        SetStatus("Novo parágrafo adicionado com a formatação do anterior.");
    }

    private static async Task<List<(string Text, bool Bold, bool Italic, bool Underline, int FontSize, AvaloniaTextAlignment Alignment)>> ReadWordDocumentAsync(IStorageFile file)
    {
        await using var stream = await file.OpenReadAsync();
        using var document = WordprocessingDocument.Open(stream, false);

        var body = document.MainDocumentPart?.Document?.Body;
        var result = new List<(string, bool, bool, bool, int, AvaloniaTextAlignment)>();
        if (body is null) return result;

        foreach (var paragraph in body.Descendants<Paragraph>())
        {
            var text = string.Concat(paragraph.Descendants<Text>().Select(t => t.Text));
            var runProperties = paragraph.Descendants<RunProperties>().FirstOrDefault();
            var alignment = paragraph.ParagraphProperties?.Justification?.Val?.Value switch
            {
                JustificationValues.Center => AvaloniaTextAlignment.Center,
                JustificationValues.Right or JustificationValues.End => AvaloniaTextAlignment.Right,
                JustificationValues.Both or JustificationValues.Distribute => AvaloniaTextAlignment.Justify,
                _ => AvaloniaTextAlignment.Left
            };

            var bold = runProperties?.Bold is not null && runProperties.Bold.Val?.Value != false;
            var italic = runProperties?.Italic is not null && runProperties.Italic.Val?.Value != false;
            var underline = runProperties?.Underline is not null &&
                runProperties.Underline.Val?.Value != UnderlineValues.None;

            var fontSize = 12;
            var sizeValue = runProperties?.FontSize?.Val?.Value;
            if (int.TryParse(sizeValue, out var halfPoints) && halfPoints > 0)
                fontSize = Math.Clamp((int)Math.Round(halfPoints / 2.0), 8, 72);

            result.Add((text, bold, italic, underline, fontSize, alignment));
        }

        return result;
    }

    private async Task SaveWordDocumentAsync()
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

                foreach (var paragraph in _writerParagraphs)
                {
                    var lines = (paragraph.Editor.Text ?? string.Empty)
                        .Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');

                    foreach (var line in lines)
                    {
                        var justification = paragraph.Alignment switch
                        {
                            AvaloniaTextAlignment.Center => JustificationValues.Center,
                            AvaloniaTextAlignment.Right => JustificationValues.Right,
                            AvaloniaTextAlignment.Justify => JustificationValues.Both,
                            _ => JustificationValues.Left
                        };

                        var p = new Paragraph(new ParagraphProperties(new Justification { Val = justification }));
                        var runProperties = new RunProperties();
                        if (paragraph.IsBold) runProperties.AppendChild(new Bold());
                        if (paragraph.IsItalic) runProperties.AppendChild(new Italic());
                        if (paragraph.IsUnderline) runProperties.AppendChild(new Underline { Val = UnderlineValues.Single });
                        runProperties.AppendChild(new FontSize { Val = (paragraph.FontSize * 2).ToString(CultureInfo.InvariantCulture) });

                        var run = new Run();
                        run.AppendChild(runProperties);
                        run.AppendChild(new Text(line) { Space = SpaceProcessingModeValues.Preserve });
                        p.AppendChild(run);
                        body.AppendChild(p);
                    }
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
            var file = await PickOpenFileAsync("Abrir documento de texto", new FilePickerFileType("Arquivos de texto")
            {
                Patterns = new[] { "*.txt", "*.md" }
            });

            if (file is null) return;
            SetWriterText(await ReadTextAsync(file));
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
            var paragraphs = await ReadWordDocumentAsync(file);
            SetWriterText(string.Empty);
            WriterParagraphs.Children.Clear();
            _writerParagraphs.Clear();

            foreach (var item in paragraphs)
            {
                var paragraph = AddWriterParagraph(item.Text, false);
                paragraph.IsBold = item.Bold;
                paragraph.IsItalic = item.Italic;
                paragraph.IsUnderline = item.Underline;
                paragraph.FontSize = item.FontSize;
                paragraph.Alignment = item.Alignment;
                ApplyParagraphVisualStyle(paragraph);
            }

            if (_writerParagraphs.Count == 0)
                AddWriterParagraph(string.Empty, false);

            _activeParagraph = _writerParagraphs.FirstOrDefault();
            SetStatus($"Documento Word aberto: {file.Name}. Formatação básica de parágrafo importada.");
        }
        catch (Exception ex)
        {
            SetStatus($"Não foi possível abrir o documento Word: {ex.Message}");
        }
    }

    private async void SaveWriter_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        await SaveTextAsync("Salvar documento de texto", "Documento.txt", GetWriterText(),
            new FilePickerFileType("Texto") { Patterns = new[] { "*.txt" } });
    }

    private async void SaveWord_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        await SaveWordDocumentAsync();
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
