# UpgradeOFFICE

Uma suíte de produtividade web em desenvolvimento, com interface em português.

## Prévia atual

- **Upgrade Writer:** editor de texto com formatação básica, contagem de palavras e exportação HTML.
- **Upgrade Sheets:** grade editável de 15 × 8 células, fórmulas aritméticas simples e exportação CSV.
- **Upgrade Slides:** criar, editar e remover slides com título e subtítulo.
- **Meus arquivos:** listar, buscar, abrir e excluir trabalhos locais.
- **Início:** painel de acesso rápido e trabalhos recentes.

## Como executar

Abra `index.html` em um navegador moderno. Não é necessário instalar dependências ou executar um servidor.

## Armazenamento e privacidade

Esta versão salva os dados no `localStorage` do navegador atual. Os documentos não são enviados a um servidor nem sincronizados entre dispositivos. Limpar os dados do navegador pode apagar os arquivos; exporte cópias importantes.

## Tecnologias

HTML, CSS e JavaScript puro, sem dependências de terceiros em tempo de execução. A fonte tipográfica pode ser carregada do Google Fonts; se não houver conexão, o navegador usa fontes locais alternativas.

## Limitações conhecidas

Esta é uma prova de conceito inicial. Ainda não há contas, sincronização na nuvem, colaboração, importação de formatos Microsoft Office, exportação PPTX/XLSX/DOCX nem compatibilidade completa com fórmulas de planilhas.