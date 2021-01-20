CREATE TABLE [dbo].[stations] (
    [station_code]         INT           NULL,
    [station_call_letters] VARCHAR (15)  NOT NULL,
    [affiliation]          VARCHAR (7)   NULL,
    [market_code]          SMALLINT      NULL,
    [legacy_call_letters]  VARCHAR (15)  NOT NULL,
    [modified_by]          VARCHAR (63)  NOT NULL,
    [modified_date]        DATETIME      NOT NULL,
    [id]                   INT           IDENTITY (1, 1) NOT NULL,
    [rep_firm_name]        VARCHAR (100) NULL,
    [owner_name]           VARCHAR (100) NULL,
    [is_true_ind]          BIT           NOT NULL,
    CONSTRAINT [PK_stations] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_stations_markets] FOREIGN KEY ([market_code]) REFERENCES [dbo].[markets] ([market_code])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_stations_markets]
    ON [dbo].[stations]([market_code] ASC);

