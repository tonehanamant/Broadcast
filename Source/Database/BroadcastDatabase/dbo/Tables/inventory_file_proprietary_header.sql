CREATE TABLE [dbo].[inventory_file_proprietary_header] (
    [id]                       INT              IDENTITY (1, 1) NOT NULL,
    [inventory_file_id]        INT              NOT NULL,
    [effective_date]           DATETIME         NOT NULL,
    [end_date]                 DATETIME         NOT NULL,
    [cpm]                      DECIMAL (19, 4)  NULL,
    [audience_id]              INT              NULL,
    [contracted_daypart_id]    INT              NULL,
    [share_projection_book_id] INT              NULL,
    [hut_projection_book_id]   INT              NULL,
    [playback_type]            INT              NULL,
    [nti_to_nsi_increase]      DECIMAL (18, 10) NULL,
    [standard_daypart_id]      INT              NULL,
    CONSTRAINT [PK_inventory_file_proprietary_header] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_inventory_file_proprietary_header_audiences] FOREIGN KEY ([audience_id]) REFERENCES [dbo].[audiences] ([id]),
    CONSTRAINT [FK_inventory_file_proprietary_header_dayparts] FOREIGN KEY ([contracted_daypart_id]) REFERENCES [dbo].[dayparts] ([id]),
    CONSTRAINT [FK_inventory_file_proprietary_header_inventory_files] FOREIGN KEY ([inventory_file_id]) REFERENCES [dbo].[inventory_files] ([id]),
    CONSTRAINT [FK_inventory_file_proprietary_header_media_months] FOREIGN KEY ([share_projection_book_id]) REFERENCES [dbo].[media_months] ([id]),
    CONSTRAINT [FK_inventory_file_proprietary_header_media_months_hut_book] FOREIGN KEY ([hut_projection_book_id]) REFERENCES [dbo].[media_months] ([id]),
    CONSTRAINT [FK_inventory_file_proprietary_header_standard_dayparts] FOREIGN KEY ([standard_daypart_id]) REFERENCES [dbo].[standard_dayparts] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_inventory_file_proprietary_header_standard_dayparts]
    ON [dbo].[inventory_file_proprietary_header]([standard_daypart_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_inventory_file_proprietary_header_media_months_hut_book]
    ON [dbo].[inventory_file_proprietary_header]([hut_projection_book_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_inventory_file_proprietary_header_media_months]
    ON [dbo].[inventory_file_proprietary_header]([share_projection_book_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_inventory_file_proprietary_header_inventory_files]
    ON [dbo].[inventory_file_proprietary_header]([inventory_file_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_inventory_file_proprietary_header_dayparts]
    ON [dbo].[inventory_file_proprietary_header]([contracted_daypart_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_inventory_file_proprietary_header_audiences]
    ON [dbo].[inventory_file_proprietary_header]([audience_id] ASC);

