-- MicroFin.Auth - Schema de criação da tabela users
-- Banco: AuthDB

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'AuthDB')
    CREATE DATABASE AuthDB;
GO

USE AuthDB;
GO

-- Criar tabela de usuários se não existir
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Users')
BEGIN
    CREATE TABLE Users
    (
        Id UNIQUEIDENTIFIER PRIMARY KEY NOT NULL,
        Email NVARCHAR(256) NOT NULL,
        PasswordHash NVARCHAR(MAX) NOT NULL,
        CreatedAtUtc DATETIME2 NOT NULL,
        CONSTRAINT UQ_Users_Email UNIQUE (Email)
    );

    -- Índice para busca por email
    CREATE INDEX IX_Users_Email ON Users(Email);

    PRINT 'Tabela Users criada com sucesso.';
END
ELSE
BEGIN
    PRINT 'Tabela Users já existe.';
END
GO
