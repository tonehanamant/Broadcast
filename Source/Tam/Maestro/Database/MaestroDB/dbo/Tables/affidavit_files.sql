CREATE TABLE [dbo].[affidavit_files] (
    [id]                              INT             IDENTITY (1, 1) NOT NULL,
    [file_size]                       INT             NOT NULL,
    [file_date]                       DATETIME        NOT NULL,
    [file_type]                       TINYINT         NOT NULL,
    [checkin_date]                    DATETIME        NOT NULL,
    [load_date]                       DATETIME        NULL,
    [status]                          CHAR (10)       NOT NULL,
    [original_filename]               VARCHAR (255)   NOT NULL,
    [checkin_filename]                VARCHAR (255)   NOT NULL,
    [load_duration]                   INT             NULL,
    [hash]                            CHAR (59)       NOT NULL,
    [total_checkin_invoice_count]     INT             NOT NULL,
    [total_checkin_affidavit_count]   INT             NOT NULL,
    [total_loaded_invoice_count]      INT             NULL,
    [total_loaded_affidavit_count]    INT             NULL,
    [total_duplicate_invoice_count]   INT             NULL,
    [total_duplicate_affidavit_count] INT             NULL,
    [physical_file]                   VARBINARY (MAX) NOT NULL,
    [forced_system_id]                INT             NULL,
    [business_unit_id]                TINYINT         CONSTRAINT [DF_affidavit_files_business_unit_id] DEFAULT ((1)) NOT NULL,
    [addressable_percentage]          FLOAT (53)      NULL,
    CONSTRAINT [PK_invoice_files] PRIMARY KEY CLUSTERED ([id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_affidavit_files_business_units] FOREIGN KEY ([business_unit_id]) REFERENCES [dbo].[business_units] ([id]),
    CONSTRAINT [FK_affidavit_files_systems] FOREIGN KEY ([forced_system_id]) REFERENCES [dbo].[systems] ([id]),
    CONSTRAINT [IX_affidavit_files] UNIQUE NONCLUSTERED ([hash] ASC) WITH (FILLFACTOR = 90)
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Contains and keeps track of all received electronic affidavit files.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_files';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'Affidavit Files', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_files';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'Used to keep track of electronic affidavit files, their check-in, and their loading statisstics.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_files';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Unique Identifier', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_files', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_files', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The size of the file in bytes.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_files', @level2type = N'COLUMN', @level2name = N'file_size';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'Determined during check-in.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_files', @level2type = N'COLUMN', @level2name = N'file_size';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The date the file was last modified.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_files', @level2type = N'COLUMN', @level2name = N'file_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'Determined during check-in.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_files', @level2type = N'COLUMN', @level2name = N'file_date';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The type of affidavit file.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_files', @level2type = N'COLUMN', @level2name = N'file_type';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'These numbers correspond to enumeration. 0=Spot By Spot, 1=Spot Data, 2=eMedia, 3=Cablevision, 4=DirectTV', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_files', @level2type = N'COLUMN', @level2name = N'file_type';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The timestamp that the file was checked-in.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_files', @level2type = N'COLUMN', @level2name = N'checkin_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_files', @level2type = N'COLUMN', @level2name = N'checkin_date';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The timestamp that the file was loaded.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_files', @level2type = N'COLUMN', @level2name = N'load_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'NULL until the file is loaded.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_files', @level2type = N'COLUMN', @level2name = N'load_date';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The status of an affidavit.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_files', @level2type = N'COLUMN', @level2name = N'status';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'Possible values: ''Checked In'' or ''Loaded''', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_files', @level2type = N'COLUMN', @level2name = N'status';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The original unmodified file name of the file.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_files', @level2type = N'COLUMN', @level2name = N'original_filename';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_files', @level2type = N'COLUMN', @level2name = N'original_filename';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The newly created file name of the file.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_files', @level2type = N'COLUMN', @level2name = N'checkin_filename';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'Determined during check-in.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_files', @level2type = N'COLUMN', @level2name = N'checkin_filename';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The amount of time it took to load the file.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_files', @level2type = N'COLUMN', @level2name = N'load_duration';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'In seconds. NULL until the file is loaded.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_files', @level2type = N'COLUMN', @level2name = N'load_duration';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_files', @level2type = N'COLUMN', @level2name = N'hash';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_files', @level2type = N'COLUMN', @level2name = N'hash';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_files', @level2type = N'COLUMN', @level2name = N'total_checkin_invoice_count';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_files', @level2type = N'COLUMN', @level2name = N'total_checkin_invoice_count';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_files', @level2type = N'COLUMN', @level2name = N'total_checkin_affidavit_count';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_files', @level2type = N'COLUMN', @level2name = N'total_checkin_affidavit_count';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_files', @level2type = N'COLUMN', @level2name = N'total_loaded_invoice_count';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_files', @level2type = N'COLUMN', @level2name = N'total_loaded_invoice_count';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_files', @level2type = N'COLUMN', @level2name = N'total_loaded_affidavit_count';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_files', @level2type = N'COLUMN', @level2name = N'total_loaded_affidavit_count';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_files', @level2type = N'COLUMN', @level2name = N'total_duplicate_invoice_count';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_files', @level2type = N'COLUMN', @level2name = N'total_duplicate_invoice_count';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_files', @level2type = N'COLUMN', @level2name = N'total_duplicate_affidavit_count';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_files', @level2type = N'COLUMN', @level2name = N'total_duplicate_affidavit_count';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_files', @level2type = N'COLUMN', @level2name = N'physical_file';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_files', @level2type = N'COLUMN', @level2name = N'physical_file';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_files', @level2type = N'COLUMN', @level2name = N'forced_system_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_files', @level2type = N'COLUMN', @level2name = N'forced_system_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_files', @level2type = N'COLUMN', @level2name = N'business_unit_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_files', @level2type = N'COLUMN', @level2name = N'business_unit_id';

