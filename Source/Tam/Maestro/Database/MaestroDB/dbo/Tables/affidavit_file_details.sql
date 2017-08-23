CREATE TABLE [dbo].[affidavit_file_details] (
    [id]                        INT IDENTITY (1, 1) NOT NULL,
    [system_id]                 INT NOT NULL,
    [affidavit_file_id]         INT NOT NULL,
    [media_month_id]            INT NOT NULL,
    [checkin_invoice_count]     INT NOT NULL,
    [checkin_affadavit_count]   INT NOT NULL,
    [loaded_invoice_count]      INT NULL,
    [loaded_affidavit_count]    INT NULL,
    [duplicate_invoice_count]   INT NULL,
    [duplicate_affidavit_count] INT NULL,
    CONSTRAINT [PK_affidavit_file_dtls] PRIMARY KEY CLUSTERED ([id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_affidavit_file_details_systems] FOREIGN KEY ([system_id]) REFERENCES [dbo].[systems] ([id]),
    CONSTRAINT [FK_affidavit_file_dtls_affidavit_files] FOREIGN KEY ([affidavit_file_id]) REFERENCES [dbo].[affidavit_files] ([id]),
    CONSTRAINT [FK_affidavit_file_dtls_media_months] FOREIGN KEY ([media_month_id]) REFERENCES [dbo].[media_months] ([id]),
    CONSTRAINT [IX_affidavit_file_details] UNIQUE NONCLUSTERED ([affidavit_file_id] ASC, [media_month_id] ASC, [system_id] ASC) WITH (FILLFACTOR = 90)
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'A reference to a media month that was in the affidavit file.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_file_details', @level2type = N'COLUMN', @level2name = N'media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_file_details', @level2type = N'COLUMN', @level2name = N'media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_file_details', @level2type = N'COLUMN', @level2name = N'checkin_invoice_count';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_file_details', @level2type = N'COLUMN', @level2name = N'checkin_invoice_count';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_file_details', @level2type = N'COLUMN', @level2name = N'checkin_affadavit_count';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_file_details', @level2type = N'COLUMN', @level2name = N'checkin_affadavit_count';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_file_details', @level2type = N'COLUMN', @level2name = N'loaded_invoice_count';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_file_details', @level2type = N'COLUMN', @level2name = N'loaded_invoice_count';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_file_details', @level2type = N'COLUMN', @level2name = N'loaded_affidavit_count';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_file_details', @level2type = N'COLUMN', @level2name = N'loaded_affidavit_count';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_file_details', @level2type = N'COLUMN', @level2name = N'duplicate_invoice_count';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_file_details', @level2type = N'COLUMN', @level2name = N'duplicate_invoice_count';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_file_details', @level2type = N'COLUMN', @level2name = N'duplicate_affidavit_count';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_file_details', @level2type = N'COLUMN', @level2name = N'duplicate_affidavit_count';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Contains the unique systems and zones that were in an electronic affidavit file.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_file_details';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'Affidavit File Details', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_file_details';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'Use this table to lookup what systems and zones were in a given electronic affidavit file. These records are determined during check-in.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_file_details';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Unique Identifier', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_file_details', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_file_details', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'A reference to a system that was in the affidavit file.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_file_details', @level2type = N'COLUMN', @level2name = N'system_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_file_details', @level2type = N'COLUMN', @level2name = N'system_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'A reference to the associated parent affidavit file record.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_file_details', @level2type = N'COLUMN', @level2name = N'affidavit_file_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_file_details', @level2type = N'COLUMN', @level2name = N'affidavit_file_id';

