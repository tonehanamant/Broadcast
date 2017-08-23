CREATE TABLE [dbo].[affidavits] (
    [id]                    BIGINT       IDENTITY (1, 1) NOT NULL,
    [media_month_id]        INT          NOT NULL,
    [status_id]             TINYINT      NOT NULL,
    [invoice_id]            INT          NOT NULL,
    [traffic_id]            INT          NULL,
    [material_id]           INT          NULL,
    [zone_id]               INT          NULL,
    [network_id]            INT          NULL,
    [spot_length_id]        TINYINT      NULL,
    [air_date]              DATETIME     NULL,
    [air_time]              INT          NULL,
    [rate]                  INT          NULL,
    [affidavit_file_line]   INT          NOT NULL,
    [affidavit_air_date]    VARCHAR (31) NOT NULL,
    [affidavit_air_time]    VARCHAR (31) NOT NULL,
    [affidavit_length]      VARCHAR (15) NOT NULL,
    [affidavit_copy]        VARCHAR (63) NOT NULL,
    [affidavit_net]         VARCHAR (15) NOT NULL,
    [affidavit_syscode]     VARCHAR (15) NOT NULL,
    [affidavit_rate]        VARCHAR (15) NOT NULL,
    [hash]                  CHAR (59)    NULL,
    [subscribers]           INT          NULL,
    [program_name]          VARCHAR (63) CONSTRAINT [DF_affidavits_program_name] DEFAULT ('') NOT NULL,
    [adjusted_air_date]     DATE         NULL,
    [adjusted_air_time]     INT          NULL,
    [gmt_air_datetime]      DATETIME     NULL,
    [gracenote_schedule_id] INT          NULL,
    CONSTRAINT [PK_affidavits] PRIMARY KEY CLUSTERED ([media_month_id] ASC, [id] ASC) WITH (FILLFACTOR = 90) ON [MediaMonthScheme] ([media_month_id]),
    CONSTRAINT [FK_affidavits_materials] FOREIGN KEY ([material_id]) REFERENCES [dbo].[materials] ([id]),
    CONSTRAINT [FK_affidavits_networks] FOREIGN KEY ([network_id]) REFERENCES [dbo].[networks] ([id]),
    CONSTRAINT [FK_affidavits_traffic] FOREIGN KEY ([traffic_id]) REFERENCES [dbo].[traffic] ([id]),
    CONSTRAINT [FK_affidavits_zones] FOREIGN KEY ([zone_id]) REFERENCES [dbo].[zones] ([id])
);


GO
ALTER TABLE [dbo].[affidavits] NOCHECK CONSTRAINT [FK_affidavits_traffic];




GO
ALTER TABLE [dbo].[affidavits] NOCHECK CONSTRAINT [FK_affidavits_traffic];


GO
CREATE NONCLUSTERED INDEX [IX_affidavits_post]
    ON [dbo].[affidavits]([media_month_id] ASC, [status_id] ASC, [material_id] ASC, [air_date] ASC)
    INCLUDE([id], [invoice_id], [zone_id], [network_id], [air_time], [subscribers], [rate], [program_name]) WITH (FILLFACTOR = 90)
    ON [MediaMonthScheme] ([media_month_id]);


GO
CREATE NONCLUSTERED INDEX [IX_affidavits_networks]
    ON [dbo].[affidavits]([media_month_id] ASC, [network_id] ASC)
    INCLUDE([affidavit_net], [invoice_id], [affidavit_file_line]) WITH (FILLFACTOR = 90)
    ON [MediaMonthScheme] ([media_month_id]);


GO
CREATE NONCLUSTERED INDEX [IX_affidavits_invoices]
    ON [dbo].[affidavits]([media_month_id] ASC, [invoice_id] ASC)
    INCLUDE([id]) WITH (FILLFACTOR = 90)
    ON [MediaMonthScheme] ([media_month_id]);


GO
CREATE NONCLUSTERED INDEX [IX_affidavits_invoice_hashes]
    ON [dbo].[affidavits]([media_month_id] ASC, [invoice_id] ASC)
    INCLUDE([hash]) WITH (FILLFACTOR = 90)
    ON [MediaMonthScheme] ([media_month_id]);


GO
CREATE NONCLUSTERED INDEX [IX_affidavit_traffic_id]
    ON [dbo].[affidavits]([traffic_id] ASC) WITH (FILLFACTOR = 90)
    ON [MediaMonthScheme] ([media_month_id]);


GO
CREATE NONCLUSTERED INDEX [IX_affidavits_materials]
    ON [dbo].[affidavits]([media_month_id] ASC, [material_id] ASC)
    INCLUDE([affidavit_copy], [invoice_id], [affidavit_file_line], [id]) WITH (FILLFACTOR = 90)
    ON [MediaMonthScheme] ([media_month_id]);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Contains all affidavits contained in electronic affidavit files.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'Affidavits', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Unique Identifier', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits', @level2type = N'COLUMN', @level2name = N'media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits', @level2type = N'COLUMN', @level2name = N'media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'A reference to the status record.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits', @level2type = N'COLUMN', @level2name = N'status_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits', @level2type = N'COLUMN', @level2name = N'status_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'A reference to the invoice this affidavit belongs to.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits', @level2type = N'COLUMN', @level2name = N'invoice_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits', @level2type = N'COLUMN', @level2name = N'invoice_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'A reference to the traffic order this affidavit is a part of.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits', @level2type = N'COLUMN', @level2name = N'traffic_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits', @level2type = N'COLUMN', @level2name = N'traffic_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'A reference to the copy record.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits', @level2type = N'COLUMN', @level2name = N'material_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits', @level2type = N'COLUMN', @level2name = N'material_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'A reference to the zone record.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits', @level2type = N'COLUMN', @level2name = N'zone_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits', @level2type = N'COLUMN', @level2name = N'zone_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'A reference to the network record.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits', @level2type = N'COLUMN', @level2name = N'network_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits', @level2type = N'COLUMN', @level2name = N'network_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits', @level2type = N'COLUMN', @level2name = N'spot_length_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits', @level2type = N'COLUMN', @level2name = N'spot_length_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits', @level2type = N'COLUMN', @level2name = N'air_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits', @level2type = N'COLUMN', @level2name = N'air_date';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits', @level2type = N'COLUMN', @level2name = N'air_time';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits', @level2type = N'COLUMN', @level2name = N'air_time';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits', @level2type = N'COLUMN', @level2name = N'rate';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits', @level2type = N'COLUMN', @level2name = N'rate';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The line number of the affidavit in the original electronic affidavit file.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits', @level2type = N'COLUMN', @level2name = N'affidavit_file_line';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits', @level2type = N'COLUMN', @level2name = N'affidavit_file_line';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The air date of the affidavit.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits', @level2type = N'COLUMN', @level2name = N'affidavit_air_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits', @level2type = N'COLUMN', @level2name = N'affidavit_air_date';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The air time of the affidavit.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits', @level2type = N'COLUMN', @level2name = N'affidavit_air_time';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'In military time.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits', @level2type = N'COLUMN', @level2name = N'affidavit_air_time';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The spot length of the affidavit.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits', @level2type = N'COLUMN', @level2name = N'affidavit_length';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits', @level2type = N'COLUMN', @level2name = N'affidavit_length';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The copy of the affidavit.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits', @level2type = N'COLUMN', @level2name = N'affidavit_copy';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits', @level2type = N'COLUMN', @level2name = N'affidavit_copy';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The network of the affidavit.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits', @level2type = N'COLUMN', @level2name = N'affidavit_net';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits', @level2type = N'COLUMN', @level2name = N'affidavit_net';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The system code of the affidavit.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits', @level2type = N'COLUMN', @level2name = N'affidavit_syscode';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits', @level2type = N'COLUMN', @level2name = N'affidavit_syscode';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The rate of the affidavit.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits', @level2type = N'COLUMN', @level2name = N'affidavit_rate';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits', @level2type = N'COLUMN', @level2name = N'affidavit_rate';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'SHA1 Encrypted value representing the entire textual line from the original affidavit line in the affidavit file.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits', @level2type = N'COLUMN', @level2name = N'hash';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'This should always be unique as it is used to prevent duplicates.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits', @level2type = N'COLUMN', @level2name = N'hash';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits', @level2type = N'COLUMN', @level2name = N'subscribers';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits', @level2type = N'COLUMN', @level2name = N'subscribers';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits', @level2type = N'COLUMN', @level2name = N'program_name';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits', @level2type = N'COLUMN', @level2name = N'program_name';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits', @level2type = N'COLUMN', @level2name = N'adjusted_air_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits', @level2type = N'COLUMN', @level2name = N'adjusted_air_date';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits', @level2type = N'COLUMN', @level2name = N'adjusted_air_time';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits', @level2type = N'COLUMN', @level2name = N'adjusted_air_time';

