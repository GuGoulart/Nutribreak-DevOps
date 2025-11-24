# NutriBreak API

> **Global Solution 2025 - FIAP**  
> **Disciplina:** DevOps Tools & Cloud Computing  
> **Autor:** Gustavo Bretas | **RM:** 555708

---

## Sobre o Projeto

**NutriBreak** é uma API REST desenvolvida em .NET 8 para gerenciamento de refeições e pausas de bem-estar, implementando uma **pipeline CI/CD completa** usando **Azure DevOps**.

### Objetivos

- ✅ Implementar pipeline CI/CD automatizada
- ✅ Gerenciar projeto com Azure Boards
- ✅ Versionar código com Azure Repos
- ✅ Build e testes automatizados (8 testes unitários)
- ✅ Deploy automatizado no Azure App Service
- ✅ Integração com Azure SQL Database

---

## Tecnologias Utilizadas

### **Backend**
- **.NET 8.0** - Framework principal
- **ASP.NET Core Web API** - Construção da API REST
- **Entity Framework Core 8.0** - ORM
- **SQL Server** - Banco de dados

### **Testes**
- **xUnit** - Framework de testes (8 testes unitários)
- **FluentAssertions** - Assertions fluentes
- **Moq** - Mocking
- **InMemory Database** - Testes isolados

### **DevOps**
- **Azure DevOps** - Boards, Repos, Pipelines
- **Azure App Service** - Hospedagem da API
- **Azure SQL Database** - Banco de dados em nuvem
- **YAML Pipelines** - Infraestrutura como código

### **Recursos da API**
- **API Versioning** - Versionamento de endpoints
- **Swagger/OpenAPI** - Documentação interativa
- **HATEOAS** - Hypermedia as the Engine of Application State
- **Health Checks** - Monitoramento de saúde

---

## 🔄 Pipeline CI/CD - Script Completo

### **arquivo: `azure-pipelines.yml`**

```yaml
# =====================================================
# NUTRIBREAK API - PIPELINE CI/CD
# Autor: Gustavo Bretas (RM 555708)
# Descrição: Pipeline completa de Build, Test e Deploy
# =====================================================

trigger:
  branches:
    include:
      - main
      - master
  paths:
    exclude:
      - README.md
      - docs/**
      - .gitignore

variables:
  buildConfiguration: 'Release'
  dotnetSdkVersion: '8.x'
  azureSubscription: 'MyAzureSubscription'
  webAppName: 'nutribreak-api-16762'
  resourceGroupName: 'rg-nutribreak'

stages:
  # ===================================================
  # STAGE 1: BUILD AND TEST (CI)
  # ===================================================
  - stage: BuildAndTest
    displayName: 'Build and Test (CI)'
    jobs:
      - job: Build
        displayName: 'Build, Test and Publish'
        pool:
          vmImage: 'ubuntu-latest'
        
        steps:
          # Step 1: Instalar .NET SDK
          - task: UseDotNet@2
            displayName: 'Install .NET SDK $(dotnetSdkVersion)'
            inputs:
              packageType: 'sdk'
              version: '$(dotnetSdkVersion)'
              installationPath: $(Agent.ToolsDirectory)/dotnet

          # Step 2: Restore de pacotes NuGet
          - task: DotNetCoreCLI@2
            displayName: 'Restore NuGet Packages'
            inputs:
              command: 'restore'
              projects: '**/*.csproj'
              feedsToUse: 'select'

          # Step 3: Build da aplicação
          - task: DotNetCoreCLI@2
            displayName: 'Build Application'
            inputs:
              command: 'build'
              projects: '**/*.csproj'
              arguments: '--configuration $(buildConfiguration) --no-restore'

          # Step 4: Executar testes unitários
          - task: DotNetCoreCLI@2
            displayName: 'Run Unit Tests'
            inputs:
              command: 'test'
              projects: '**/Tests/*.csproj'
              arguments: '--configuration $(buildConfiguration) --no-build --logger trx --collect:"XPlat Code Coverage"'
              publishTestResults: true

          # Step 5: Publicar resultados dos testes
          - task: PublishTestResults@2
            displayName: 'Publish Test Results'
            condition: succeededOrFailed()
            inputs:
              testResultsFormat: 'VSTest'
              testResultsFiles: '**/*.trx'
              mergeTestResults: true
              failTaskOnFailedTests: true
              testRunTitle: 'Unit Tests - $(Build.BuildNumber)'

          # Step 6: Publicar cobertura de código
          - task: PublishCodeCoverageResults@2
            displayName: 'Publish Code Coverage'
            condition: succeededOrFailed()
            inputs:
              codeCoverageTool: 'Cobertura'
              summaryFileLocation: '$(Agent.TempDirectory)/**/coverage.cobertura.xml'
              failIfCoverageEmpty: false

          # Step 7: Publicar aplicação
          - task: DotNetCoreCLI@2
            displayName: 'Publish Application'
            inputs:
              command: 'publish'
              publishWebProjects: true
              arguments: '--configuration $(buildConfiguration) --output $(Build.ArtifactStagingDirectory)/publish --no-build'
              zipAfterPublish: true
              modifyOutputPath: false

          # Step 8: Publicar artifact
          - task: PublishBuildArtifacts@1
            displayName: 'Publish Build Artifact'
            inputs:
              PathtoPublish: '$(Build.ArtifactStagingDirectory)/publish'
              ArtifactName: 'nutribreak-drop'
              publishLocation: 'Container'

  # ===================================================
  # STAGE 2: DEPLOY TO AZURE (CD)
  # ===================================================
  - stage: DeployToAzure
    displayName: 'Deploy to Azure (CD)'
    dependsOn: BuildAndTest
    condition: succeeded()
    jobs:
      - deployment: DeployWeb
        displayName: 'Deploy to Azure App Service'
        environment: 'production'
        pool:
          vmImage: 'ubuntu-latest'
        
        strategy:
          runOnce:
            deploy:
              steps:
                # Step 1: Download do artifact
                - task: DownloadBuildArtifacts@1
                  displayName: 'Download Build Artifact'
                  inputs:
                    buildType: 'current'
                    downloadType: 'single'
                    artifactName: 'nutribreak-drop'
                    downloadPath: '$(System.ArtifactsDirectory)'

                # Step 2: Deploy para Azure App Service
                - task: AzureWebApp@1
                  displayName: 'Deploy to Azure App Service'
                  inputs:
                    azureSubscription: '$(azureSubscription)'
                    appType: 'webApp'
                    appName: '$(webAppName)'
                    package: '$(System.ArtifactsDirectory)/nutribreak-drop/**/*.zip'
                    deploymentMethod: 'zipDeploy'

                # Step 3: Restart do App Service
                - task: AzureAppServiceManage@0
                  displayName: 'Restart Azure App Service'
                  inputs:
                    azureSubscription: '$(azureSubscription)'
                    action: 'Restart Azure App Service'
                    webAppName: '$(webAppName)'
                    resourceGroupName: '$(resourceGroupName)'

  # ===================================================
  # STAGE 3: SMOKE TESTS (OPCIONAL)
  # ===================================================
  - stage: SmokeTests
    displayName: 'Smoke Tests'
    dependsOn: DeployToAzure
    condition: succeeded()
    jobs:
      - job: ValidateDeployment
        displayName: 'Validate API Health'
        pool:
          vmImage: 'ubuntu-latest'
        
        steps:
          # Health Check da API
          - task: PowerShell@2
            displayName: 'Validate API Health Endpoint'
            inputs:
              targetType: 'inline'
              script: |
                $response = Invoke-WebRequest -Uri "https://$(webAppName).azurewebsites.net/health" -UseBasicParsing
                if ($response.StatusCode -eq 200) {
                  Write-Host "✅ Health check passed!"
                  exit 0
                } else {
                  Write-Error "❌ Health check failed! Status: $($response.StatusCode)"
                  exit 1
                }
```

---

## 🗄️ Script SQL - Criação do Banco de Dados

### **Executar no Azure SQL Database:**

```sql
-- =====================================================
-- NUTRIBREAK DATABASE - SCRIPT DE CRIAÇÃO
-- Autor: Gustavo Bretas (RM 555708)
-- Data: Novembro 2025
-- =====================================================

-- =====================================================
-- TABELA: Users (Usuários)
-- =====================================================
CREATE TABLE Users (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(200) NOT NULL,
    Email NVARCHAR(200) NOT NULL UNIQUE,
    WorkMode NVARCHAR(50) NOT NULL CHECK (WorkMode IN ('office', 'remote', 'hybrid')),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    
    CONSTRAINT CK_Users_Email CHECK (Email LIKE '%@%.%')
);

CREATE INDEX IX_Users_Email ON Users(Email);
CREATE INDEX IX_Users_WorkMode ON Users(WorkMode);

-- =====================================================
-- TABELA: Meals (Refeições)
-- =====================================================
CREATE TABLE Meals (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Calories INT NOT NULL CHECK (Calories > 0 AND Calories <= 5000),
    TimeOfDay NVARCHAR(50) NOT NULL CHECK (TimeOfDay IN ('breakfast', 'lunch', 'snack', 'dinner')),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    
    CONSTRAINT FK_Meals_Users FOREIGN KEY (UserId) 
        REFERENCES Users(Id) ON DELETE CASCADE
);

CREATE INDEX IX_Meals_UserId ON Meals(UserId);
CREATE INDEX IX_Meals_TimeOfDay ON Meals(TimeOfDay);
CREATE INDEX IX_Meals_CreatedAt ON Meals(CreatedAt DESC);

-- =====================================================
-- TABELA: BreakRecords (Pausas de Bem-Estar)
-- =====================================================
CREATE TABLE BreakRecords (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    Type NVARCHAR(50) NOT NULL CHECK (Type IN ('stretching', 'meditation', 'quick', 'breathing', 'walk')),
    DurationMinutes INT NOT NULL CHECK (DurationMinutes > 0 AND DurationMinutes <= 120),
    Mood NVARCHAR(50) NOT NULL CHECK (Mood IN ('relaxed', 'calm', 'energized', 'focused', 'refreshed', 'stressed', 'tired')),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    
    CONSTRAINT FK_BreakRecords_Users FOREIGN KEY (UserId) 
        REFERENCES Users(Id) ON DELETE CASCADE
);

CREATE INDEX IX_BreakRecords_UserId ON BreakRecords(UserId);
CREATE INDEX IX_BreakRecords_Type ON BreakRecords(Type);
CREATE INDEX IX_BreakRecords_Mood ON BreakRecords(Mood);
CREATE INDEX IX_BreakRecords_CreatedAt ON BreakRecords(CreatedAt DESC);

-- =====================================================
-- VERIFICAÇÃO FINAL
-- =====================================================
SELECT 'Users' AS TableName, COUNT(*) AS RecordCount FROM Users
UNION ALL
SELECT 'Meals', COUNT(*) FROM Meals
UNION ALL
SELECT 'BreakRecords', COUNT(*) FROM BreakRecords;
```

---

## 🌐 Ambientes

### **Produção (Azure)**
- **API:** https://nutribreak-api-16762.azurewebsites.net
- **Swagger:** https://nutribreak-api-16762.azurewebsites.net/swagger
- **Health Check:** https://nutribreak-api-16762.azurewebsites.net/health

### **Banco de Dados (Azure SQL)**
```
Server: nutribreak-sqlserver-12829.database.windows.net
Database: NutriBreakDB
Port: 1433
```

### **Azure DevOps**
- **Organização:** https://dev.azure.com/gustavogbm
- **Projeto:** nutribreak-DevOp
- **Repositório:** Nutribreak-DevOps-git

---

## 🔧 Configuração e Execução Local

### **Pré-requisitos**
- .NET 8 SDK
- Visual Studio 2022 ou VS Code
- SQL Server ou Azure SQL Database
- Git

### **1️⃣ Clonar o Repositório**

```bash
# Via HTTPS
git clone https://dev.azure.com/gustavogbm/nutribreak-DevOp/_git/Nutribreak-DevOps-git

cd Nutribreak-DevOps-git
```

### **2️⃣ Configurar Connection String**

Edite `NutriBreak/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=NutriBreakDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### **3️⃣ Criar Banco de Dados**

Execute o script SQL acima no seu SQL Server local ou Azure SQL Database.

### **4️⃣ Executar a Aplicação**

```bash
# Restaurar pacotes
dotnet restore

# Build
dotnet build

# Executar
cd NutriBreak
dotnet run
```

**Acessar:**
- API: `https://localhost:7000`
- Swagger: `https://localhost:7000/swagger`

### **5️⃣ Executar Testes**

```bash
cd Tests
dotnet test

# Com cobertura
dotnet test --collect:"XPlat Code Coverage"
```

## 🧪 Guia Completo de Demonstração

### **PASSO 1: Verificar Health Check (30 segundos)**

```bash
# Testar saúde da API
curl -X GET https://nutribreak-api-16762.azurewebsites.net/health
```

**Resultado esperado:**
```json
{
  "status": "Healthy"
}
```

---

### **PASSO 2: CRUD de Usuários (3 minutos)**

#### **2.1 Criar Usuários**

```bash
# Criar João - Trabalho Presencial
curl -X POST https://nutribreak-api-16762.azurewebsites.net/api/v1/users \
  -H "Content-Type: application/json" \
  -d '{
    "name": "João Silva",
    "email": "joao.silva@fiap.com.br",
    "workMode": "office"
  }'

# Criar Maria - Trabalho Remoto
curl -X POST https://nutribreak-api-16762.azurewebsites.net/api/v1/users \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Maria Santos",
    "email": "maria.santos@fiap.com.br",
    "workMode": "remote"
  }'

# Criar Pedro - Trabalho Híbrido
curl -X POST https://nutribreak-api-16762.azurewebsites.net/api/v1/users \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Pedro Oliveira",
    "email": "pedro.oliveira@fiap.com.br",
    "workMode": "hybrid"
  }'
```

**⚠️ IMPORTANTE:** Copie o `id` (GUID) retornado do João!

#### **2.2 Listar Usuários**

```bash
# Listar todos (primeira página)
curl -X GET "https://nutribreak-api-16762.azurewebsites.net/api/v1/users?pageNumber=1&pageSize=10"
```

#### **2.3 Buscar Usuário por ID**

```bash
# Substituir {id} pelo GUID do João
curl -X GET https://nutribreak-api-16762.azurewebsites.net/api/v1/users/{id}
```

#### **2.4 Atualizar Usuário**

```bash
# Atualizar João (mudar para remote)
curl -X PUT https://nutribreak-api-16762.azurewebsites.net/api/v1/users/{id-joao} \
  -H "Content-Type: application/json" \
  -d '{
    "name": "João Silva Santos",
    "workMode": "remote"
  }'
```

#### **2.5 Deletar Usuário**

```bash
# Deletar Pedro
curl -X DELETE https://nutribreak-api-16762.azurewebsites.net/api/v1/users/{id-pedro}
```

---

### **PASSO 3: CRUD de Refeições (2 minutos)**

```bash
# Criar Café da Manhã
curl -X POST https://nutribreak-api-16762.azurewebsites.net/api/v1/meals \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "{id-do-joao}",
    "title": "Café da Manhã Saudável",
    "calories": 350,
    "timeOfDay": "breakfast"
  }'

# Criar Almoço
curl -X POST https://nutribreak-api-16762.azurewebsites.net/api/v1/meals \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "{id-do-joao}",
    "title": "Almoço Completo",
    "calories": 650,
    "timeOfDay": "lunch"
  }'

# Listar Refeições
curl -X GET https://nutribreak-api-16762.azurewebsites.net/api/v1/meals
```

---

### **PASSO 4: CRUD de Pausas (2 minutos)**

```bash
# Criar Alongamento
curl -X POST https://nutribreak-api-16762.azurewebsites.net/api/v1/break-records \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "{id-do-joao}",
    "type": "stretching",
    "durationMinutes": 10,
    "mood": "relaxed"
  }'

# Criar Meditação
curl -X POST https://nutribreak-api-16762.azurewebsites.net/api/v1/break-records \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "{id-do-joao}",
    "type": "meditation",
    "durationMinutes": 15,
    "mood": "calm"
  }'

# Listar Pausas
curl -X GET https://nutribreak-api-16762.azurewebsites.net/api/v1/break-records
```

---

### **PASSO 5: Verificar Dados no Banco (1 minuto)**

Conecte no Azure SQL Database e execute:

```sql
-- Ver todos os usuários
SELECT * FROM Users;

-- Ver todas as refeições
SELECT * FROM Meals ORDER BY CreatedAt DESC;

-- Ver todas as pausas
SELECT * FROM BreakRecords ORDER BY CreatedAt DESC;

-- Estatísticas por usuário
SELECT 
    u.Name,
    u.Email,
    u.WorkMode,
    COUNT(DISTINCT m.Id) as TotalMeals,
    SUM(m.Calories) as TotalCalories,
    COUNT(DISTINCT b.Id) as TotalBreaks,
    SUM(b.DurationMinutes) as TotalBreakMinutes
FROM Users u
LEFT JOIN Meals m ON m.UserId = u.Id
LEFT JOIN BreakRecords b ON b.UserId = u.Id
GROUP BY u.Id, u.Name, u.Email, u.WorkMode;
```

---

## 🧪 Testes Unitários

### **Cobertura**
- ✅ **8 testes** implementados
- ✅ **100% de aprovação** na pipeline
- ✅ Code Coverage publicado

### **Testes Implementados**

1. `DbContext_DevePermitirAdicionarUsuario`
2. `DbContext_DevePermitirListarUsuarios`
3. `DbContext_DevePermitirAtualizarUsuario`
4. `DbContext_DevePermitirDeletarUsuario`
5. `DbContext_DevePermitirAdicionarMeal`
6. `DbContext_DevePermitirListarMeals`
7. `DbContext_DevePermitirAdicionarBreakRecord`
8. `DbContext_DevePermitirListarBreakRecords`

### **Executar Testes Localmente**

```bash
# Todos os testes
dotnet test

# Com cobertura
dotnet test --collect:"XPlat Code Coverage"

# Com detalhes
dotnet test --logger "console;verbosity=detailed"
```

## 📚 Links e Referências

### **Projeto**
- **Azure DevOps:** https://dev.azure.com/gustavogbm/nutribreak-DevOp
- **API Swagger:** https://nutribreak-api-16762.azurewebsites.net/swagger
- **Portal Azure:** https://portal.azure.com

---

## 👨‍💻 Autores
- Alice Teixeira Caldeira - RM 556293
- Leonardo Cadena de Souza - RM 557528
- Gustavo Goulart Bretas - RM 555708

---

**🚀 NutriBreak API - Desenvolvido com ❤️ para FIAP Global Solution 2025**
