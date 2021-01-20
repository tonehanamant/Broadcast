CREATE TABLE [dbo].[plan_version_blackout_markets] (
    [id]                      INT        IDENTITY (1, 1) NOT NULL,
    [market_code]             SMALLINT   NOT NULL,
    [market_coverage_file_id] INT        NOT NULL,
    [rank]                    INT        NOT NULL,
    [percentage_of_us]        FLOAT (53) NOT NULL,
    [plan_version_id]         INT        NOT NULL,
    CONSTRAINT [PK_plan_version_blackout_markets] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_plan_blackout_markets_market_coverage_file] FOREIGN KEY ([market_coverage_file_id]) REFERENCES [dbo].[market_coverage_files] ([id]),
    CONSTRAINT [FK_plan_blackout_markets_markets] FOREIGN KEY ([market_code]) REFERENCES [dbo].[markets] ([market_code]),
    CONSTRAINT [FK_plan_version_blackout_markets_plan_versions] FOREIGN KEY ([plan_version_id]) REFERENCES [dbo].[plan_versions] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_plan_version_blackout_markets_plan_versions]
    ON [dbo].[plan_version_blackout_markets]([plan_version_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_plan_blackout_markets_markets]
    ON [dbo].[plan_version_blackout_markets]([market_code] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_plan_blackout_markets_market_coverage_file]
    ON [dbo].[plan_version_blackout_markets]([market_coverage_file_id] ASC);

