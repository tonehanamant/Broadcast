CREATE TABLE [dbo].[broadcast_affidavits] (
    [media_month_id]              INT           NOT NULL,
    [id]                          BIGINT        IDENTITY (1, 1) NOT NULL,
    [broadcast_affidavit_file_id] INT           NOT NULL,
    [spot_length_id]              INT           NULL,
    [station]                     VARCHAR (63)  NOT NULL,
    [market]                      VARCHAR (63)  NOT NULL,
    [network_affilates]           VARCHAR (63)  NOT NULL,
    [air_date]                    DATETIME      NOT NULL,
    [air_time]                    INT           NOT NULL,
    [isci]                        VARCHAR (63)  NOT NULL,
    [program]                     VARCHAR (255) NOT NULL,
    [phone_number]                VARCHAR (25)  NOT NULL,
    [campaign]                    VARCHAR (63)  NOT NULL,
    [advertiser]                  VARCHAR (127) NOT NULL,
    [product]                     VARCHAR (127) NOT NULL,
    [invoice_number]              VARCHAR (63)  NOT NULL,
    [market_rank]                 INT           NULL,
    CONSTRAINT [PK_broadcast_affidavits] PRIMARY KEY CLUSTERED ([media_month_id] ASC, [id] ASC) WITH (FILLFACTOR = 90) ON [MediaMonthScheme] ([media_month_id]),
    CONSTRAINT [FK_broadcast_affidavits_broadcast_affidavit_files] FOREIGN KEY ([broadcast_affidavit_file_id]) REFERENCES [dbo].[broadcast_affidavit_files] ([id]),
    CONSTRAINT [FK_broadcast_affidavits_media_months] FOREIGN KEY ([media_month_id]) REFERENCES [dbo].[media_months] ([id]),
    CONSTRAINT [FK_broadcast_affidavits_spot_lengths] FOREIGN KEY ([spot_length_id]) REFERENCES [dbo].[spot_lengths] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_affidavits';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_affidavits';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_affidavits';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_affidavits', @level2type = N'COLUMN', @level2name = N'media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_affidavits', @level2type = N'COLUMN', @level2name = N'media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_affidavits', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_affidavits', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_affidavits', @level2type = N'COLUMN', @level2name = N'broadcast_affidavit_file_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_affidavits', @level2type = N'COLUMN', @level2name = N'broadcast_affidavit_file_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_affidavits', @level2type = N'COLUMN', @level2name = N'spot_length_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_affidavits', @level2type = N'COLUMN', @level2name = N'spot_length_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_affidavits', @level2type = N'COLUMN', @level2name = N'station';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_affidavits', @level2type = N'COLUMN', @level2name = N'station';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_affidavits', @level2type = N'COLUMN', @level2name = N'market';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_affidavits', @level2type = N'COLUMN', @level2name = N'market';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_affidavits', @level2type = N'COLUMN', @level2name = N'network_affilates';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_affidavits', @level2type = N'COLUMN', @level2name = N'network_affilates';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_affidavits', @level2type = N'COLUMN', @level2name = N'air_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_affidavits', @level2type = N'COLUMN', @level2name = N'air_date';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_affidavits', @level2type = N'COLUMN', @level2name = N'air_time';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_affidavits', @level2type = N'COLUMN', @level2name = N'air_time';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_affidavits', @level2type = N'COLUMN', @level2name = N'isci';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_affidavits', @level2type = N'COLUMN', @level2name = N'isci';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_affidavits', @level2type = N'COLUMN', @level2name = N'program';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_affidavits', @level2type = N'COLUMN', @level2name = N'program';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_affidavits', @level2type = N'COLUMN', @level2name = N'phone_number';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_affidavits', @level2type = N'COLUMN', @level2name = N'phone_number';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_affidavits', @level2type = N'COLUMN', @level2name = N'campaign';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_affidavits', @level2type = N'COLUMN', @level2name = N'campaign';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_affidavits', @level2type = N'COLUMN', @level2name = N'advertiser';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_affidavits', @level2type = N'COLUMN', @level2name = N'advertiser';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_affidavits', @level2type = N'COLUMN', @level2name = N'product';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_affidavits', @level2type = N'COLUMN', @level2name = N'product';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_affidavits', @level2type = N'COLUMN', @level2name = N'invoice_number';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_affidavits', @level2type = N'COLUMN', @level2name = N'invoice_number';

