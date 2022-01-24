CREATE TABLE [dbo].[plan_version_daypart_available_markets] (
    [id]                             INT        IDENTITY (1, 1) NOT NULL,
    [plan_version_daypart_goal_id]   INT        NOT NULL,
    [market_code]                    SMALLINT   NOT NULL,
    [market_coverage_file_id]        INT        NOT NULL,
    [share_of_voice_percent]         FLOAT (53) NULL,
    [is_user_share_of_voice_percent] BIT        NOT NULL,
    CONSTRAINT [FK_plan_version_daypart_available_markets_market_coverage_file] FOREIGN KEY ([market_coverage_file_id]) REFERENCES [dbo].[market_coverage_files] ([id]),
    CONSTRAINT [FK_plan_version_daypart_available_markets_markets] FOREIGN KEY ([market_code]) REFERENCES [dbo].[markets] ([market_code]),
    CONSTRAINT [FK_plan_version_daypart_available_markets_plan_version_daypart_goals] FOREIGN KEY ([plan_version_daypart_goal_id]) REFERENCES [dbo].[plan_version_daypart_goals] ([id]) ON DELETE CASCADE
);

