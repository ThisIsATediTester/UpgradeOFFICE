# UpgradeOFFICE

**Aplicativo de escritório desktop em C# + Avalonia UI**, com interface em português e foco em ferramentas locais de produtividade.

> Estado: protótipo inicial em desenvolvimento. Ainda não é uma suíte de escritório completa nem um instalador pronto para distribuição.

## Plataformas planejadas
- Windows
- Linux
- macOS

## Módulos iniciais
- **Writer:** edição básica de texto, com abertura e salvamento de arquivos TXT.
- **Sheets:** edição simples de conteúdo CSV, com abertura e salvamento de arquivos CSV.
- **Slides:** rascunho de apresentação com título e conteúdo, salvo como TXT.

## Privacidade e segurança
- Não implementa telemetria, anúncios, analytics nem comunicação com servidores.
- Os arquivos são abertos e salvos localmente, por meio dos seletores de arquivo do sistema operacional.
- O aplicativo não solicita privilégios administrativos.
- Dependências NuGet estão declaradas no arquivo do projeto para facilitar auditoria.

## Requisitos
- SDK do .NET 8 ou posterior.
- Acesso à internet pode ser necessário na primeira restauração dos pacotes NuGet.

## Executar localmente

Na pasta raiz do repositório:

```bash
dotnet restore UpgradeOFFICE.Desktop/UpgradeOFFICE.Desktop.csproj
dotnet run --project UpgradeOFFICE.Desktop/UpgradeOFFICE.Desktop.csproj
```

## Publicar

Execute os comandos no sistema operacional ou ambiente de compilação correspondente.

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

No macOS, distribuir o aplicativo fora do ambiente de desenvolvimento pode exigir assinatura e notarização.

## Limitações atuais
- A planilha ainda não possui uma grade de células nem fórmulas.
- As apresentações ainda não são exportadas para PPTX.
- O Writer ainda é um editor de texto simples, sem suporte completo a DOCX.
- A compilação e os testes em Windows, Linux e macOS ainda precisam ser executados.

O desenvolvimento principal da versão desktop está na branch `avalonia-desktop`.
