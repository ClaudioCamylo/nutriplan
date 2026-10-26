# NutriPlan

Planejador de dieta e controle de peso baseado em cálculo calórico e análise combinatória.
Trabalho A3 – Garantia da Qualidade de Software (GQS) · Prof. Daniel Henrique Matos de Paiva.

**ODS 3 – Saúde e Bem-Estar** (secundária: ODS 2 – Fome Zero).

## Equipe

| Nome | GitHub | RA |
|------|--------|----|
| Claudio Camilo Junio | [@claudio3009](https://github.com/claudio3009) | [preencher RA] |
| Matheus Moreira Lopes | [@mmoreiralopes8-sketch](https://github.com/mmoreiralopes8-sketch) | [preencher RA] |
| Gabriel Ribeiro de Oliveira | [@gabrielrrbeiro-byte](https://github.com/gabrielrrbeiro-byte) | [preencher RA] |
| Luiz Felipe Chaves Vaz | [@LuizFvazz](https://github.com/LuizFvazz) | [preencher RA] |
| Claudio Camylo | [@ClaudioCamylo](https://github.com/ClaudioCamylo) | [preencher RA] |

Os nomes e RAs exibidos no site ficam em `src/NutriPlan.Web/appsettings.json` (seção `Team`).

## Como rodar

Requisito: [.NET SDK 10](https://dotnet.microsoft.com/download).

```bash
dotnet run --project src/NutriPlan.Web
```

Abra http://localhost:5000 (ou a porta exibida no terminal). O banco SQLite é criado sozinho em
`src/NutriPlan.Web/App_Data/nutriplan.db`.

## Testes

```bash
dotnet test
```

## O que o sistema faz

- Cadastro de perfil (sexo, idade, peso, altura, atividade, objetivo e meta semanal).
- Cálculo de **IMC**, **TMB** (Mifflin-St Jeor), **gasto diário** (TMB × fator de atividade) e **meta calórica**
  (déficit de 500 kcal para emagrecer, superávit de 500 kcal para ganhar, ou manutenção).
- **Cardápio de 7 dias** com 4 refeições (25% café, 35% almoço, 15% lanche, 25% jantar), montado com
  **análise combinatória** C(n, k): gera combinações de 2 a 6 alimentos, filtra as que ficam perto da meta da refeição,
  exige equilíbrio (ex.: almoço com carboidrato, proteína e vegetal) e sorteia sem repetir o prato do dia anterior.
- **Metas semanais e conquistas**: ao registrar o peso, o sistema avalia a meta, entrega emblemas
  (primeira conquista, semana cumprida, sequências de 2/4/8 semanas), recalcula o plano e, após duas semanas sem cumprir,
  aprofunda o déficit/superávit em 100 kcal (limite de 300).
- **Consulta de alimentos** na base local (SQLite, valores da TACO) e na API externa **Open Food Facts**.
- Regras de segurança: somente maiores de 18 anos, piso de 1.500 kcal (homens) e 1.200 kcal (mulheres),
  meta semanal de 0,1 a 1 kg.

Exemplo do trabalho: homem, 19 anos, 1,70 m, 112 kg, sedentário, emagrecer → IMC 38,8 · TMB 2.092 · gasto 2.511 · meta 2.011 kcal.

## Estrutura

```
NutriPlan/
├── src/
│   ├── NutriPlan.Core/      # regras de negócio puras (cálculos, dieta, conquistas, validação)
│   └── NutriPlan.Web/       # ASP.NET Core Razor Pages, SQLite, cliente Open Food Facts
├── tests/NutriPlan.Tests/   # xUnit (TDD)
├── scripts/gitflow.ps1      # cria branches e commits semânticos do GitFlow
└── .github/workflows/ci.yml # integração contínua (GitHub Actions)
```

## Boas práticas de GQS aplicadas

| Critério | Como atendemos |
|----------|----------------|
| Versionamento | GitFlow (`main`, `develop`, `feature/*`) e commits semânticos (Conventional Commits) |
| Testes unitários (TDD) | xUnit, testes escritos antes das regras de cálculo, dieta, conquistas e validação |
| CI | GitHub Actions: restore, `dotnet format`, build e testes a cada push e pull request |
| Código limpo | Regras de negócio isoladas no `Core`, métodos pequenos, nomes claros |
| Tratamento de erros | Validação de entradas, `try/catch` no banco e na API, mensagens amigáveis na tela |
| Indentação | 4 espaços, imposta por `.editorconfig` e verificada com `dotnet format` na pipeline |
| Diferenciais | Banco SQLite e API externa Open Food Facts |

## Fontes de dados

- Tabela Brasileira de Composição de Alimentos (TACO) – NEPA/Unicamp (valores aproximados na base inicial).
- Open Food Facts – https://openfoodfacts.github.io/documentation/
- Equação de Mifflin-St Jeor (1990).

> Ferramenta educacional. Não substitui a orientação de um nutricionista ou médico.
