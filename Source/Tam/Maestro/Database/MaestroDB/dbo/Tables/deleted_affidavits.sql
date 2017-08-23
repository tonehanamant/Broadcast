CREATE TABLE [dbo].[deleted_affidavits] (
    [id]                  BIGINT       NOT NULL,
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
    [program_name]        VARCHAR (63) CONSTRAINT [DF_deleted_affidavits_program_name] DEFAULT ('') NOT NULL,
    [adjusted_air_date]   DATE         NULL,
    [adjusted_air_time]   INT          NULL,
    CONSTRAINT [PK_deleted_affidavits] PRIMARY KEY CLUSTERED ([id] ASC, [media_month_id] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_affidavits';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_affidavits';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_affidavits';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_affidavits', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_affidavits', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_affidavits', @level2type = N'COLUMN', @level2name = N'media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_affidavits', @level2type = N'COLUMN', @level2name = N'media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_affidavits', @level2type = N'COLUMN', @level2name = N'status_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_affidavits', @level2type = N'COLUMN', @level2name = N'status_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_affidavits', @level2type = N'COLUMN', @level2name = N'invoice_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_affidavits', @level2type = N'COLUMN', @level2name = N'invoice_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_affidavits', @level2type = N'COLUMN', @level2name = N'traffic_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_affidavits', @level2type = N'COLUMN', @level2name = N'traffic_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_affidavits', @level2type = N'COLUMN', @level2name = N'material_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_affidavits', @level2type = N'COLUMN', @level2name = N'material_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_affidavits', @level2type = N'COLUMN', @level2name = N'zone_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_affidavits', @level2type = N'COLUMN', @level2name = N'zone_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_affidavits', @level2type = N'COLUMN', @level2name = N'network_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_affidavits', @level2type = N'COLUMN', @level2name = N'network_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_affidavits', @level2type = N'COLUMN', @level2name = N'spot_length_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_affidavits', @level2type = N'COLUMN', @level2name = N'spot_length_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_affidavits', @level2type = N'COLUMN', @level2name = N'air_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_affidavits', @level2type = N'COLUMN', @level2name = N'air_date';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_affidavits', @level2type = N'COLUMN', @level2name = N'air_time';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_affidavits', @level2type = N'COLUMN', @level2name = N'air_time';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_affidavits', @level2type = N'COLUMN', @level2name = N'rate';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_affidavits', @level2type = N'COLUMN', @level2name = N'rate';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_affidavits', @level2type = N'COLUMN', @level2name = N'affidavit_file_line';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_affidavits', @level2type = N'COLUMN', @level2name = N'affidavit_file_line';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_affidavits', @level2type = N'COLUMN', @level2name = N'affidavit_air_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_affidavits', @level2type = N'COLUMN', @level2name = N'affidavit_air_date';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_affidavits', @level2type = N'COLUMN', @level2name = N'affidavit_air_time';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_affidavits', @level2type = N'COLUMN', @level2name = N'affidavit_air_time';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_affidavits', @level2type = N'COLUMN', @level2name = N'affidavit_length';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_affidavits', @level2type = N'COLUMN', @level2name = N'affidavit_length';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_affidavits', @level2type = N'COLUMN', @level2name = N'affidavit_copy';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_affidavits', @level2type = N'COLUMN', @level2name = N'affidavit_copy';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_affidavits', @level2type = N'COLUMN', @level2name = N'affidavit_net';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_affidavits', @level2type = N'COLUMN', @level2name = N'affidavit_net';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_affidavits', @level2type = N'COLUMN', @level2name = N'affidavit_syscode';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_affidavits', @level2type = N'COLUMN', @level2name = N'affidavit_syscode';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_affidavits', @level2type = N'COLUMN', @level2name = N'affidavit_rate';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_affidavits', @level2type = N'COLUMN', @level2name = N'affidavit_rate';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_affidavits', @level2type = N'COLUMN', @level2name = N'hash';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_affidavits', @level2type = N'COLUMN', @level2name = N'hash';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_affidavits', @level2type = N'COLUMN', @level2name = N'subscribers';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_affidavits', @level2type = N'COLUMN', @level2name = N'subscribers';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_affidavits', @level2type = N'COLUMN', @level2name = N'program_name';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_affidavits', @level2type = N'COLUMN', @level2name = N'program_name';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_affidavits', @level2type = N'COLUMN', @level2name = N'adjusted_air_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_affidavits', @level2type = N'COLUMN', @level2name = N'adjusted_air_date';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_affidavits', @level2type = N'COLUMN', @level2name = N'adjusted_air_time';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_affidavits', @level2type = N'COLUMN', @level2name = N'adjusted_air_time';

