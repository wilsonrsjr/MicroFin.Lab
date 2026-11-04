-- MicroFin.Customers - Schema de criação da tabela ProcessedMessages
-- Banco: MicroFin
-- Descrição: Tabela para idempotência de mensagens RabbitMQ (evitar débito duplicado)

USE MicroFin;
GO

-- Criar tabela de mensagens processadas se não existir
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ProcessedMessages' AND TABLE_SCHEMA = 'dbo')
BEGIN
    CREATE TABLE ProcessedMessages
    (
        MessageId NVARCHAR(255) PRIMARY KEY NOT NULL,
        CustomerId UNIQUEIDENTIFIER NOT NULL,
        Amount DECIMAL(18, 2) NOT NULL,
        ProcessedAtUtc DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        -- Chave estrangeira para Customers
        CONSTRAINT FK_ProcessedMessages_Customers FOREIGN KEY (CustomerId) 
            REFERENCES Customers(Id) ON DELETE CASCADE
    );

    -- Índice para busca por CustomerId (para auditoria)
    CREATE INDEX IX_ProcessedMessages_CustomerId ON ProcessedMessages(CustomerId);

    -- Índice para busca por ProcessedAtUtc (para limpeza periódica)
    CREATE INDEX IX_ProcessedMessages_ProcessedAtUtc ON ProcessedMessages(ProcessedAtUtc);

    PRINT 'Tabela ProcessedMessages criada com sucesso.';
END
ELSE
BEGIN
    PRINT 'Tabela ProcessedMessages já existe.';
END
GO
