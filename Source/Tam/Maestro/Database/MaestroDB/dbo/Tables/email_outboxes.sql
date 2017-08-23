CREATE TABLE [dbo].[email_outboxes] (
    [id]                     INT           IDENTITY (1, 1) NOT FOR REPLICATION NOT NULL,
    [email_profile_id]       INT           NOT NULL,
    [subject]                VARCHAR (255) NOT NULL,
    [body]                   TEXT          NOT NULL,
    [is_html]                BIT           NOT NULL,
    [mail_priority]          TINYINT       NOT NULL,
    [reply_to_email_address] VARCHAR (255) NULL,
    [date_created]           DATETIME      NOT NULL,
    CONSTRAINT [PK_email_outbox] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_email_outbox_email_profiles] FOREIGN KEY ([email_profile_id]) REFERENCES [dbo].[email_profiles] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'email_outboxes';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'email_outboxes';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'email_outboxes';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'email_outboxes', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'email_outboxes', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'email_outboxes', @level2type = N'COLUMN', @level2name = N'email_profile_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'email_outboxes', @level2type = N'COLUMN', @level2name = N'email_profile_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'email_outboxes', @level2type = N'COLUMN', @level2name = N'subject';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'email_outboxes', @level2type = N'COLUMN', @level2name = N'subject';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'email_outboxes', @level2type = N'COLUMN', @level2name = N'body';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'email_outboxes', @level2type = N'COLUMN', @level2name = N'body';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'email_outboxes', @level2type = N'COLUMN', @level2name = N'is_html';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'email_outboxes', @level2type = N'COLUMN', @level2name = N'is_html';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'email_outboxes', @level2type = N'COLUMN', @level2name = N'mail_priority';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'email_outboxes', @level2type = N'COLUMN', @level2name = N'mail_priority';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'email_outboxes', @level2type = N'COLUMN', @level2name = N'reply_to_email_address';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'email_outboxes', @level2type = N'COLUMN', @level2name = N'reply_to_email_address';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'email_outboxes', @level2type = N'COLUMN', @level2name = N'date_created';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'email_outboxes', @level2type = N'COLUMN', @level2name = N'date_created';

