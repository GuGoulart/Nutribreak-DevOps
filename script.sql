-- =====================================================
-- SCRIPT DDL - NUTRIBREAK DATABASE
-- Projeto: NutriBreak - Global Solution 2025
-- Disciplina: DevOps Tools & Cloud Computing
-- Objetivo: Criar estrutura completa do banco de dados
-- =====================================================

-- =====================================================
-- 1. CRIAR DATABASE (se não existir)
-- =====================================================
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'NutriBreakDB')
BEGIN
    CREATE DATABASE NutriBreakDB;
    PRINT 'Database NutriBreakDB criada com sucesso!';
END
GO

USE NutriBreakDB;
GO

-- =====================================================
-- 2. TABELA: Users
-- Armazena usuários da plataforma
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        Name NVARCHAR(100) NOT NULL,
        Email NVARCHAR(200) NOT NULL UNIQUE,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL,
        
        CONSTRAINT CK_Users_Email CHECK (Email LIKE '%@%.%')
    );
    
    CREATE INDEX IX_Users_Email ON Users(Email);
    CREATE INDEX IX_Users_CreatedAt ON Users(CreatedAt DESC);
    
    PRINT 'Tabela Users criada com sucesso!';
END
GO

-- =====================================================
-- 3. TABELA: Meals
-- Armazena refeições dos usuários
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Meals')
BEGIN
    CREATE TABLE Meals (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId UNIQUEIDENTIFIER NOT NULL,
        Title NVARCHAR(120) NOT NULL,
        Description NVARCHAR(500) NULL,
        Calories INT NOT NULL,
        TimeOfDay NVARCHAR(50) NOT NULL,
        ConsumedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT FK_Meals_Users FOREIGN KEY (UserId) 
            REFERENCES Users(Id) ON DELETE CASCADE,
        CONSTRAINT CK_Meals_Calories CHECK (Calories >= 0 AND Calories <= 5000),
        CONSTRAINT CK_Meals_TimeOfDay CHECK (TimeOfDay IN ('breakfast', 'lunch', 'snack', 'dinner'))
    );
    
    CREATE INDEX IX_Meals_UserId ON Meals(UserId);
    CREATE INDEX IX_Meals_ConsumedAt ON Meals(ConsumedAt DESC);
    CREATE INDEX IX_Meals_TimeOfDay ON Meals(TimeOfDay);
    
    PRINT 'Tabela Meals criada com sucesso!';
END
GO

-- =====================================================
-- 4. TABELA: BreakRecords
-- Armazena registros de pausas/breaks dos usuários
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BreakRecords')
BEGIN
    CREATE TABLE BreakRecords (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId UNIQUEIDENTIFIER NOT NULL,
        Type NVARCHAR(50) NOT NULL,
        Mood NVARCHAR(50) NULL,
        DurationMinutes INT NOT NULL,
        StartedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        EndedAt DATETIME2 NULL,
        Notes NVARCHAR(500) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT FK_BreakRecords_Users FOREIGN KEY (UserId) 
            REFERENCES Users(Id) ON DELETE CASCADE,
        CONSTRAINT CK_BreakRecords_Duration CHECK (DurationMinutes >= 1 AND DurationMinutes <= 120),
        CONSTRAINT CK_BreakRecords_Type CHECK (Type IN ('quick', 'long', 'breathing', 'stretching', 'walk')),
        CONSTRAINT CK_BreakRecords_Mood CHECK (Mood IN ('happy', 'stressed', 'anxious', 'tired', 'normal', 'energetic'))
    );
    
    CREATE INDEX IX_BreakRecords_UserId ON BreakRecords(UserId);
    CREATE INDEX IX_BreakRecords_StartedAt ON BreakRecords(StartedAt DESC);
    CREATE INDEX IX_BreakRecords_Type ON BreakRecords(Type);
    
    PRINT 'Tabela BreakRecords criada com sucesso!';
END
GO

-- =====================================================
-- 5. TABELA: RecommendationHistory
-- Armazena histórico de recomendações da IA Generativa
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RecommendationHistory')
BEGIN
    CREATE TABLE RecommendationHistory (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        RecommendationId NVARCHAR(50) NOT NULL,
        UserId UNIQUEIDENTIFIER NOT NULL,
        RequestContext NVARCHAR(MAX) NOT NULL,
        AIResponse NVARCHAR(MAX) NOT NULL,
        ModelUsed NVARCHAR(50) NOT NULL,
        TokensUsed INT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT FK_RecommendationHistory_Users FOREIGN KEY (UserId) 
            REFERENCES Users(Id) ON DELETE CASCADE,
        CONSTRAINT CK_RecommendationHistory_TokensUsed CHECK (TokensUsed >= 0)
    );
    
    CREATE INDEX IX_RecommendationHistory_UserId ON RecommendationHistory(UserId);
    CREATE INDEX IX_RecommendationHistory_CreatedAt ON RecommendationHistory(CreatedAt DESC);
    CREATE INDEX IX_RecommendationHistory_RecommendationId ON RecommendationHistory(RecommendationId);
    
    PRINT 'Tabela RecommendationHistory criada com sucesso!';
END
GO

-- =====================================================
-- 6. DADOS DE EXEMPLO (SEED DATA) - OPCIONAL
-- =====================================================

DECLARE @SampleUserId UNIQUEIDENTIFIER = NEWID();

IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'usuario@exemplo.com')
BEGIN
    INSERT INTO Users (Id, Name, Email, CreatedAt)
    VALUES (@SampleUserId, 'Usuário Exemplo', 'usuario@exemplo.com', GETUTCDATE());
    
    INSERT INTO Meals (UserId, Title, Description, Calories, TimeOfDay, ConsumedAt)
    VALUES 
        (@SampleUserId, 'Café da Manhã Saudável', 'Pão integral com ovo e café', 350, 'breakfast', GETUTCDATE()),
        (@SampleUserId, 'Almoço Equilibrado', 'Arroz, feijão, frango grelhado e salada', 600, 'lunch', GETUTCDATE());
    
    INSERT INTO BreakRecords (UserId, Type, Mood, DurationMinutes, StartedAt)
    VALUES 
        (@SampleUserId, 'quick', 'normal', 5, GETUTCDATE()),
        (@SampleUserId, 'stretching', 'tired', 10, DATEADD(HOUR, -2, GETUTCDATE()));
    
    PRINT 'Dados de exemplo inseridos com sucesso!';
END
GO

-- =====================================================
-- 7. VERIFICAÇÃO FINAL
-- =====================================================
SELECT 
    'Users' AS Tabela, 
    COUNT(*) AS TotalRegistros,
    MAX(CreatedAt) AS UltimoRegistro
FROM Users
UNION ALL
SELECT 
    'Meals', 
    COUNT(*), 
    MAX(CreatedAt)
FROM Meals
UNION ALL
SELECT 
    'BreakRecords', 
    COUNT(*), 
    MAX(CreatedAt)
FROM BreakRecords
UNION ALL
SELECT 
    'RecommendationHistory', 
    COUNT(*), 
    MAX(CreatedAt)
FROM RecommendationHistory;
GO

PRINT '';
PRINT '=====================================================';
PRINT 'SCRIPT CONCLUÍDO COM SUCESSO!';
PRINT 'Database: NutriBreakDB';
PRINT 'Tabelas criadas: 4 (Users, Meals, BreakRecords, RecommendationHistory)';
PRINT '=====================================================';
GO