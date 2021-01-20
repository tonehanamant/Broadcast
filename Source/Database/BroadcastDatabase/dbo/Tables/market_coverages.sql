CREATE TABLE [dbo].[market_coverages] (
    [id]                      INT        IDENTITY (1, 1) NOT NULL,
    [rank]                    INT        NOT NULL,
    [tv_homes]                INT        NOT NULL,
    [percentage_of_us]        FLOAT (53) NOT NULL,
    [market_code]             SMALLINT   NOT NULL,
    [market_coverage_file_id] INT        NOT NULL,
    CONSTRAINT [PK_market_coverages] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_market_coverages_market_coverage_files] FOREIGN KEY ([market_coverage_file_id]) REFERENCES [dbo].[market_coverage_files] ([id]) ON DELETE CASCADE,
    CONSTRAINT [FK_market_coverages_markets] FOREIGN KEY ([market_code]) REFERENCES [dbo].[markets] ([market_code])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_market_coverages_markets]
    ON [dbo].[market_coverages]([market_code] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_market_coverages_market_coverage_files]
    ON [dbo].[market_coverages]([market_coverage_file_id] ASC);

