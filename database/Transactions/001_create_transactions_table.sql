-- MicroFin.Transactions - Schema de criação da tabela Transactions
-- Banco: MicroFin
-- Descrição: Tabela para armazenar transações de autorização de limite

USE MicroFin;
GO

-- Criar tabela de transações se não existir
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Transactions' AND TABLE_SCHEMA = 'dbo')
BEGIN
    CREATE TABLE Transactions
    (
        Id UNIQUEIDENTIFIER PRIMARY KEY NOT NULL DEFAULT NEWID(),
        IdCliente UNIQUEIDENTIFIER NOT NULL,
        ValorSimulacao DECIMAL(18, 2) NOT NULL,
        Status NVARCHAR(20) NOT NULL,
        CreatedAtUtc DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );

    -- Índice para busca por IdCliente
    CREATE INDEX IX_Transactions_IdCliente ON Transactions(IdCliente);

    -- Índice para busca por Status
    CREATE INDEX IX_Transactions_Status ON Transactions(Status);

    -- Índice para busca por CreatedAtUtc
    CREATE INDEX IX_Transactions_CreatedAtUtc ON Transactions(CreatedAtUtc);

    -- Índice composto para consultas por cliente e data
    CREATE INDEX IX_Transactions_IdCliente_CreatedAtUtc ON Transactions(IdCliente, CreatedAtUtc DESC);

    PRINT 'Tabela Transactions criada com sucesso.';
END
ELSE
BEGIN
    PRINT 'Tabela Transactions já existe.';
END
GO
