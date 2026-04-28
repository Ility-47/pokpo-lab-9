# pokpo-lab-9

ЛР №9: тестирование в CI/CD и анализ качества кода (GitHub Actions) для проекта **MediaShareApp**.

## Что проверяет CI

- **Static analysis**: сборка решения, проверка форматирования whitespace (`dotnet format whitespace`), анализ цикломатической сложности (`lizard`)
- **Unit tests**: запуск тестов категории `Unit` с покрытием (Coverlet)
- **Integration tests**: запуск тестов категории `Integration` с покрытием (Coverlet)
- **Coverage report**: генерация HTML-отчёта покрытия через ReportGenerator (артефакт workflow)

## Запуск локально

```powershell
dotnet restore .\lab7.sln
dotnet build .\lab7.sln --configuration Release

dotnet test .\MediaShareApp.Tests\MediaShareApp.Tests.csproj --configuration Release --filter "TestCategory=Unit"
dotnet test .\MediaShareApp.Tests\MediaShareApp.Tests.csproj --configuration Release --filter "TestCategory=Integration"
```

Покрытие + HTML отчёт:

```powershell
dotnet test .\MediaShareApp.Tests\MediaShareApp.Tests.csproj --configuration Release --filter "TestCategory=Unit" --collect:"XPlat Code Coverage" --settings .\coverlet.runsettings --results-directory .\TestResults\Unit
dotnet test .\MediaShareApp.Tests\MediaShareApp.Tests.csproj --configuration Release --filter "TestCategory=Integration" --collect:"XPlat Code Coverage" --settings .\coverlet.runsettings --results-directory .\TestResults\Integration

dotnet tool install --tool-path .\.tools dotnet-reportgenerator-globaltool
.\.tools\reportgenerator -reports:"./TestResults/**/coverage.cobertura.xml" -targetdir:"./coveragereport" -reporttypes:"Html;TextSummary"
```
