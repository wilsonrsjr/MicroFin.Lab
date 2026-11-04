-- MicroFin.Customers - Schema de criação da tabela Customers
-- Banco: MicroFin
-- Descrição: Tabela para armazenar clientes com CPF único e limite monetário

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'MicroFin')
BEGIN
    CREATE DATABASE MicroFin;
    PRINT 'Banco MicroFin criado.';
END
GO

USE MicroFin;
GO

-- Criar tabela de clientes se não existir
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Customers' AND TABLE_SCHEMA = 'dbo')
BEGIN
    CREATE TABLE Customers
    (
        Id UNIQUEIDENTIFIER PRIMARY KEY NOT NULL DEFAULT NEWID(),
        Nome NVARCHAR(255) NOT NULL,
        CPF VARCHAR(11) NOT NULL,
        ValorLimite DECIMAL(18, 2) NOT NULL,
        CreatedAtUtc DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAtUtc DATETIME2 NULL,
        CONSTRAINT UQ_Customers_CPF UNIQUE (CPF),
        CONSTRAINT CK_Customers_ValorLimite CHECK (ValorLimite >= 0)
    );

    -- Índice único para CPF (já garante unicidade, mas melhora performance de busca)
    CREATE UNIQUE INDEX IX_Customers_CPF ON Customers(CPF);

    -- Índice para ordenação por data de criação
    CREATE INDEX IX_Customers_CreatedAtUtc ON Customers(CreatedAtUtc);

    PRINT 'Tabela Customers criada com sucesso.';
END
ELSE
BEGIN
    PRINT 'Tabela Customers já existe.';
END
GO
