# UpgradeOFFICE

**Aplicativo de escritório desktop em C# + Avalonia UI**, com interface em português e foco em ferramentas locais de produtividade.

> Estado: protótipo inicial em desenvolvimento. Ainda não é uma suíte de escritório completa nem um instalador pronto para distribuição.

## Plataformas planejadas
- Windows
- Linux
- macOS

## Módulos iniciais
- **Writer:** editor de texto com abertura e salvamento de TXT e suporte inicial a documentos Word `.docx`.
- **Sheets:** edição simples de conteúdo CSV, com abertura e salvamento de arquivos CSV.
- **Slides:** rascunho de apresentação com título e conteúdo, salvo como TXT.

## Suporte a Word (.docx)
O Writer pode abrir arquivos `.docx` e importar o texto e os parágrafos, além de salvar o conteúdo editado como um novo arquivo `.docx`. O formato é gerado com o Open XML SDK.

**Limitação importante:** esta é uma primeira implementação. A edição ainda é baseada em texto simples; formatação avançada do Word, imagens, tabelas, cabeçalhos, rodapés, comentários e outros elementos não são preservados ao importar e salvar. Para evitar alterar um documento original sem querer, salve o resultado com um novo nome.

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
- O Writer ainda não substitui o Microsoft Word: não preserva toda a formatação ou os elementos complexos de documentos DOCX.
- A planilha ainda não possui uma grade de células nem fórmulas.
- As apresentações ainda não são exportadas para PPTX.
- A compilação e os testes em Windows, Linux e macOS ainda precisam ser executados.
