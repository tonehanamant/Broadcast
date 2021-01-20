CREATE TABLE [dbo].[station_month_details] (
    [station_id]       INT         NOT NULL,
    [media_month_id]   INT         NOT NULL,
    [affiliation]      VARCHAR (7) NULL,
    [market_code]      SMALLINT    NULL,
    [distributor_code] INT         NULL,
    CONSTRAINT [PK_station_month_details] PRIMARY KEY CLUSTERED ([station_id] ASC, [media_month_id] ASC),
    CONSTRAINT [FK_station_month_details_markets] FOREIGN KEY ([market_code]) REFERENCES [dbo].[markets] ([market_code]),
    CONSTRAINT [FK_station_month_details_media_months] FOREIGN KEY ([media_month_id]) REFERENCES [dbo].[media_months] ([id]),
    CONSTRAINT [FK_station_month_details_stations] FOREIGN KEY ([station_id]) REFERENCES [dbo].[stations] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_station_month_details_media_months]
    ON [dbo].[station_month_details]([media_month_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_station_month_details_markets]
    ON [dbo].[station_month_details]([market_code] ASC);

