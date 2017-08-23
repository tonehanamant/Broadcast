CREATE TABLE [dbo].[coverage_universe_comparison_market] (
    [id]               INT      IDENTITY (1, 1) NOT NULL,
    [media_month_id]   INT      NOT NULL,
    [market_id]        INT      NOT NULL,
    [reviewed_user_id] INT      NOT NULL,
    [reviewed_time]    DATETIME NOT NULL,
    CONSTRAINT [PK_coverage_universe_comparison_market] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_coverage_universe_comparison_market_media_months] FOREIGN KEY ([media_month_id]) REFERENCES [dbo].[media_months] ([id]),
    CONSTRAINT [UK_coverage_universe_comparison_market_1] UNIQUE NONCLUSTERED ([media_month_id] ASC, [market_id] ASC)
);

