CREATE TABLE [dbo].[affidavit_delivery_run_messages] (
    [affidavit_delivery_run_id] INT            NOT NULL,
    [date_created]              DATETIME       NOT NULL,
    [note]                      VARCHAR (4095) NOT NULL,
    CONSTRAINT [PK_affidavit_delivery_run_messages'] PRIMARY KEY CLUSTERED ([affidavit_delivery_run_id] ASC, [date_created] ASC),
    CONSTRAINT [FK_affidavit_delivery_run_messages_affidavit_delivery_runs] FOREIGN KEY ([affidavit_delivery_run_id]) REFERENCES [dbo].[affidavit_delivery_runs] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_delivery_run_messages';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_delivery_run_messages';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_delivery_run_messages';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_delivery_run_messages', @level2type = N'COLUMN', @level2name = N'affidavit_delivery_run_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_delivery_run_messages', @level2type = N'COLUMN', @level2name = N'affidavit_delivery_run_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_delivery_run_messages', @level2type = N'COLUMN', @level2name = N'date_created';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_delivery_run_messages', @level2type = N'COLUMN', @level2name = N'date_created';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_delivery_run_messages', @level2type = N'COLUMN', @level2name = N'note';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_delivery_run_messages', @level2type = N'COLUMN', @level2name = N'note';

