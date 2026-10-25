# UpgradeOFFICE — versão desktop

Esta branch inicia a migração para um aplicativo desktop em **C# + Avalonia UI**.

## Projeto
- Código em `UpgradeOFFICE.Desktop/`
- .NET 8
- Avalonia UI com tema Fluent
- Compatibilidade planejada: Windows, Linux e macOS
- Sem servidor, telemetria, anúncios ou chamadas de rede implementadas
- Abertura e gravação de arquivos locais por seletores nativos do sistema

## Recursos iniciais
- **Writer:** editor de texto básico; abre e salva TXT.
- **Sheets:** edição de conteúdo CSV em texto; abre e salva CSV.
- **Slides:** editor de título e conteúdo para rascunhos de slides em TXT.

Esta é uma base inicial, não um pacote de escritório completo. A planilha ainda não tem uma grade com fórmulas e os slides ainda não são exportados para PPTX. Esses recursos podem ser adicionados por etapas.

## Compilar
Instale o SDK do .NET 8 ou posterior e execute, a partir da pasta do repositório:

```bash
dotnet restore UpgradeOFFICE.Desktop/UpgradeOFFICE.Desktop.csproj
dotnet run --project UpgradeOFFICE.Desktop/UpgradeOFFICE.Desktop.csproj
```

## Publicar
Exemplos de publicação self-contained (execute no sistema ou ambiente de compilação de destino):

Windows x64:
```bash
dotnet publish UpgradeOFFICE.Desktop/UpgradeOFFICE.Desktop.csproj -c Release -r win-x64 --self-contained true
```

Linux x64:
```bash
dotnet publish UpgradeOFFICE.Desktop/UpgradeOFFICE.Desktop.csproj -c Release -r linux-x64 --self-contained true
```

macOS Apple Silicon:
```bash
dotnet publish UpgradeOFFICE.Desktop/UpgradeOFFICE.Desktop.csproj -c Release -r osx-arm64 --self-contained true
```

Para distribuir no macOS fora do ambiente de desenvolvimento, podem ser necessários assinatura e notarização. Os pacotes finais devem ser testados no sistema operacional correspondente.
