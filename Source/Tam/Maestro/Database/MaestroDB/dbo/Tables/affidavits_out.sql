CREATE TABLE [dbo].[affidavits_out] (
    [id]                  BIGINT       IDENTITY (1, 1) NOT NULL,
    [media_month_id]      INT          NOT NULL,
    [status_id]           TINYINT      NOT NULL,
    [invoice_id]          INT          NOT NULL,
    [traffic_id]          INT          NULL,
    [material_id]         INT          NULL,
    [zone_id]             INT          NULL,
    [network_id]          INT          NULL,
    [spot_length_id]      TINYINT      NULL,
    [air_date]            DATETIME     NULL,
    [air_time]            INT          NULL,
    [rate]                INT          NULL,
    [affidavit_file_line] INT          NOT NULL,
    [affidavit_air_date]  VARCHAR (31) NOT NULL,
    [affidavit_air_time]  VARCHAR (31) NOT NULL,
    [affidavit_length]    VARCHAR (15) NOT NULL,
    [affidavit_copy]      VARCHAR (63) NOT NULL,
    [affidavit_net]       VARCHAR (15) NOT NULL,
    [affidavit_syscode]   VARCHAR (15) NOT NULL,
    [affidavit_rate]      VARCHAR (15) NOT NULL,
    [hash]                CHAR (59)    NULL,
    [subscribers]         INT          NULL,
    [program_name]        VARCHAR (63) CONSTRAINT [DF_affidavits_program_name_out] DEFAULT ('') NOT NULL,
    [adjusted_air_date]   DATE         NULL,
    [adjusted_air_time]   INT          NULL,
    CONSTRAINT [PK_affidavits_out] PRIMARY KEY CLUSTERED ([media_month_id] ASC, [id] ASC) WITH (FILLFACTOR = 90) ON [MediaMonthScheme] ([media_month_id]),
    CONSTRAINT [FK_affidavits_materials_out] FOREIGN KEY ([material_id]) REFERENCES [dbo].[materials] ([id]),
    CONSTRAINT [FK_affidavits_networks_out] FOREIGN KEY ([network_id]) REFERENCES [dbo].[networks] ([id]),
    CONSTRAINT [FK_affidavits_traffic_out] FOREIGN KEY ([traffic_id]) REFERENCES [dbo].[traffic] ([id]),
    CONSTRAINT [FK_affidavits_zones_out] FOREIGN KEY ([zone_id]) REFERENCES [dbo].[zones] ([id])
);


GO
ALTER TABLE [dbo].[affidavits_out] NOCHECK CONSTRAINT [FK_affidavits_traffic_out];




GO



GO
ALTER TABLE [dbo].[affidavits_out] NOCHECK CONSTRAINT [FK_affidavits_traffic_out];


GO
CREATE NONCLUSTERED INDEX [IX_affidavits_out_post]
    ON [dbo].[affidavits_out]([media_month_id] ASC, [status_id] ASC, [material_id] ASC, [air_date] ASC)
    INCLUDE([id], [invoice_id], [zone_id], [network_id], [air_time], [subscribers], [rate], [program_name]) WITH (FILLFACTOR = 90)
    ON [MediaMonthScheme] ([media_month_id]);


GO
CREATE NONCLUSTERED INDEX [IX_affidavits_out_networks]
    ON [dbo].[affidavits_out]([media_month_id] ASC, [network_id] ASC)
    INCLUDE([affidavit_net], [invoice_id], [affidavit_file_line]) WITH (FILLFACTOR = 90)
    ON [MediaMonthScheme] ([media_month_id]);


GO
CREATE NONCLUSTERED INDEX [IX_affidavits_out_materials]
    ON [dbo].[affidavits_out]([media_month_id] ASC, [material_id] ASC)
    INCLUDE([affidavit_copy], [invoice_id], [affidavit_file_line]) WITH (FILLFACTOR = 90)
    ON [MediaMonthScheme] ([media_month_id]);


GO
CREATE NONCLUSTERED INDEX [IX_affidavits_out_invoices]
    ON [dbo].[affidavits_out]([media_month_id] ASC, [invoice_id] ASC)
    INCLUDE([id]) WITH (FILLFACTOR = 90)
    ON [MediaMonthScheme] ([media_month_id]);


GO
CREATE NONCLUSTERED INDEX [IX_affidavits_out_affidavit_traffic_id]
    ON [dbo].[affidavits_out]([traffic_id] ASC) WITH (FILLFACTOR = 90)
    ON [MediaMonthScheme] ([media_month_id]);


GO
CREATE NONCLUSTERED INDEX [IX_affidavits_out_invoice_hashes]
    ON [dbo].[affidavits_out]([media_month_id] ASC, [invoice_id] ASC)
    INCLUDE([hash]) WITH (FILLFACTOR = 90)
    ON [MediaMonthScheme] ([media_month_id]);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits_out', @level2type = N'COLUMN', @level2name = N'hash';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits_out', @level2type = N'COLUMN', @level2name = N'hash';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits_out', @level2type = N'COLUMN', @level2name = N'subscribers';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits_out', @level2type = N'COLUMN', @level2name = N'subscribers';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits_out', @level2type = N'COLUMN', @level2name = N'program_name';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits_out', @level2type = N'COLUMN', @level2name = N'program_name';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits_out', @level2type = N'COLUMN', @level2name = N'adjusted_air_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits_out', @level2type = N'COLUMN', @level2name = N'adjusted_air_date';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits_out', @level2type = N'COLUMN', @level2name = N'adjusted_air_time';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits_out', @level2type = N'COLUMN', @level2name = N'adjusted_air_time';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits_out';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits_out';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits_out';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits_out', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits_out', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits_out', @level2type = N'COLUMN', @level2name = N'media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits_out', @level2type = N'COLUMN', @level2name = N'media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits_out', @level2type = N'COLUMN', @level2name = N'status_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits_out', @level2type = N'COLUMN', @level2name = N'status_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits_out', @level2type = N'COLUMN', @level2name = N'invoice_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits_out', @level2type = N'COLUMN', @level2name = N'invoice_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits_out', @level2type = N'COLUMN', @level2name = N'traffic_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits_out', @level2type = N'COLUMN', @level2name = N'traffic_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits_out', @level2type = N'COLUMN', @level2name = N'material_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits_out', @level2type = N'COLUMN', @level2name = N'material_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits_out', @level2type = N'COLUMN', @level2name = N'zone_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits_out', @level2type = N'COLUMN', @level2name = N'zone_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits_out', @level2type = N'COLUMN', @level2name = N'network_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits_out', @level2type = N'COLUMN', @level2name = N'network_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits_out', @level2type = N'COLUMN', @level2name = N'spot_length_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits_out', @level2type = N'COLUMN', @level2name = N'spot_length_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits_out', @level2type = N'COLUMN', @level2name = N'air_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits_out', @level2type = N'COLUMN', @level2name = N'air_date';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits_out', @level2type = N'COLUMN', @level2name = N'air_time';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits_out', @level2type = N'COLUMN', @level2name = N'air_time';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits_out', @level2type = N'COLUMN', @level2name = N'rate';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits_out', @level2type = N'COLUMN', @level2name = N'rate';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits_out', @level2type = N'COLUMN', @level2name = N'affidavit_file_line';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits_out', @level2type = N'COLUMN', @level2name = N'affidavit_file_line';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits_out', @level2type = N'COLUMN', @level2name = N'affidavit_air_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits_out', @level2type = N'COLUMN', @level2name = N'affidavit_air_date';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits_out', @level2type = N'COLUMN', @level2name = N'affidavit_air_time';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits_out', @level2type = N'COLUMN', @level2name = N'affidavit_air_time';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits_out', @level2type = N'COLUMN', @level2name = N'affidavit_length';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits_out', @level2type = N'COLUMN', @level2name = N'affidavit_length';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits_out', @level2type = N'COLUMN', @level2name = N'affidavit_copy';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits_out', @level2type = N'COLUMN', @level2name = N'affidavit_copy';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits_out', @level2type = N'COLUMN', @level2name = N'affidavit_net';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits_out', @level2type = N'COLUMN', @level2name = N'affidavit_net';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits_out', @level2type = N'COLUMN', @level2name = N'affidavit_syscode';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits_out', @level2type = N'COLUMN', @level2name = N'affidavit_syscode';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits_out', @level2type = N'COLUMN', @level2name = N'affidavit_rate';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavits_out', @level2type = N'COLUMN', @level2name = N'affidavit_rate';

