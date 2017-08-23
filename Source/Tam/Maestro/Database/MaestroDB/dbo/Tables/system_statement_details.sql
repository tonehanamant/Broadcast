CREATE TABLE [dbo].[system_statement_details] (
    [system_statement_id] INT      NOT NULL,
    [date_sent]           DATETIME NOT NULL,
    [email_outbox_id]     INT      NOT NULL,
    CONSTRAINT [PK_system_statement_details] PRIMARY KEY CLUSTERED ([system_statement_id] ASC, [date_sent] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_system_statement_details_email_outbox] FOREIGN KEY ([email_outbox_id]) REFERENCES [dbo].[email_outboxes] ([id]),
    CONSTRAINT [FK_system_statement_details_system_statements] FOREIGN KEY ([system_statement_id]) REFERENCES [dbo].[system_statements] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_statement_details';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_statement_details';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_statement_details';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_statement_details', @level2type = N'COLUMN', @level2name = N'system_statement_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_statement_details', @level2type = N'COLUMN', @level2name = N'system_statement_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_statement_details', @level2type = N'COLUMN', @level2name = N'date_sent';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_statement_details', @level2type = N'COLUMN', @level2name = N'date_sent';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_statement_details', @level2type = N'COLUMN', @level2name = N'email_outbox_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_statement_details', @level2type = N'COLUMN', @level2name = N'email_outbox_id';

