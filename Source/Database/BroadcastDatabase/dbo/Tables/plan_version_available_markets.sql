CREATE TABLE [dbo].[plan_version_available_markets] (
    [id]                      INT        IDENTITY (1, 1) NOT NULL,
    [market_code]             SMALLINT   NOT NULL,
    [market_coverage_File_id] INT        NOT NULL,
    [rank]                    INT        NOT NULL,
    [percentage_of_us]        FLOAT (53) NOT NULL,
    [share_of_voice_percent]  FLOAT (53) NULL,
    [plan_version_id]         INT        NOT NULL,
    CONSTRAINT [PK_plan_version_available_markets] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_plan_available_markets_market_coverage_file] FOREIGN KEY ([market_coverage_File_id]) REFERENCES [dbo].[market_coverage_files] ([id]),
    CONSTRAINT [FK_plan_available_markets_markets] FOREIGN KEY ([market_code]) REFERENCES [dbo].[markets] ([market_code]),
    CONSTRAINT [FK_plan_version_available_markets_plan_versions] FOREIGN KEY ([plan_version_id]) REFERENCES [dbo].[plan_versions] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_plan_version_available_markets_plan_versions]
    ON [dbo].[plan_version_available_markets]([plan_version_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_plan_available_markets_markets]
    ON [dbo].[plan_version_available_markets]([market_code] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_plan_available_markets_market_coverage_file]
    ON [dbo].[plan_version_available_markets]([market_coverage_File_id] ASC);

