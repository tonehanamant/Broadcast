CREATE TABLE [dbo].[email_outbox_details] (
    [id]                INT           IDENTITY (1, 1) NOT FOR REPLICATION NOT NULL,
    [email_outbox_id]   INT           NOT NULL,
    [email_address]     VARCHAR (255) NOT NULL,
    [display_name]      VARCHAR (100) NOT NULL,
    [num_attempts]      TINYINT       NOT NULL,
    [status_code]       TINYINT       NOT NULL,
    [date_sent]         DATETIME      NULL,
    [date_last_attempt] DATETIME      NULL,
    CONSTRAINT [PK_email_outbox_details] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_email_outbox_details_email_outbox] FOREIGN KEY ([email_outbox_id]) REFERENCES [dbo].[email_outboxes] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'email_outbox_details';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'email_outbox_details';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'email_outbox_details';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'email_outbox_details', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'email_outbox_details', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'email_outbox_details', @level2type = N'COLUMN', @level2name = N'email_outbox_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'email_outbox_details', @level2type = N'COLUMN', @level2name = N'email_outbox_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'email_outbox_details', @level2type = N'COLUMN', @level2name = N'email_address';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'email_outbox_details', @level2type = N'COLUMN', @level2name = N'email_address';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'email_outbox_details', @level2type = N'COLUMN', @level2name = N'display_name';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'email_outbox_details', @level2type = N'COLUMN', @level2name = N'display_name';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'email_outbox_details', @level2type = N'COLUMN', @level2name = N'num_attempts';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'email_outbox_details', @level2type = N'COLUMN', @level2name = N'num_attempts';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'email_outbox_details', @level2type = N'COLUMN', @level2name = N'status_code';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'email_outbox_details', @level2type = N'COLUMN', @level2name = N'status_code';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'email_outbox_details', @level2type = N'COLUMN', @level2name = N'date_sent';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'email_outbox_details', @level2type = N'COLUMN', @level2name = N'date_sent';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'email_outbox_details', @level2type = N'COLUMN', @level2name = N'date_last_attempt';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'email_outbox_details', @level2type = N'COLUMN', @level2name = N'date_last_attempt';

