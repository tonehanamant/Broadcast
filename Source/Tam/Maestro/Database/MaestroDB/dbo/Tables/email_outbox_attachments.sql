CREATE TABLE [dbo].[email_outbox_attachments] (
    [id]              INT             IDENTITY (1, 1) NOT NULL,
    [email_outbox_id] INT             NOT NULL,
    [file_size]       BIGINT          NOT NULL,
    [file_name]       VARCHAR (255)   NOT NULL,
    [data]            VARBINARY (MAX) NOT NULL,
    CONSTRAINT [PK_email_outbox_attachments] PRIMARY KEY CLUSTERED ([id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_email_outbox_attachments_email_outbox] FOREIGN KEY ([email_outbox_id]) REFERENCES [dbo].[email_outboxes] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'email_outbox_attachments';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'email_outbox_attachments';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'email_outbox_attachments';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'email_outbox_attachments', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'email_outbox_attachments', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'email_outbox_attachments', @level2type = N'COLUMN', @level2name = N'email_outbox_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'email_outbox_attachments', @level2type = N'COLUMN', @level2name = N'email_outbox_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'email_outbox_attachments', @level2type = N'COLUMN', @level2name = N'file_size';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'email_outbox_attachments', @level2type = N'COLUMN', @level2name = N'file_size';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'email_outbox_attachments', @level2type = N'COLUMN', @level2name = N'file_name';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'email_outbox_attachments', @level2type = N'COLUMN', @level2name = N'file_name';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'email_outbox_attachments', @level2type = N'COLUMN', @level2name = N'data';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'email_outbox_attachments', @level2type = N'COLUMN', @level2name = N'data';

