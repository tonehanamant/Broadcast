CREATE TABLE [dbo].[campaign_sub_budgets] (
    [id]          INT            IDENTITY (1, 1) NOT NULL,
    [campaign_id] INT            NOT NULL,
    [title]       NVARCHAR (256) NOT NULL,
    [amount]      MONEY          NOT NULL,
    [start_date]  DATETIME       NOT NULL,
    [end_date]    DATETIME       NOT NULL,
    [number]      INT            NOT NULL,
    PRIMARY KEY CLUSTERED ([id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_campaign_sub_budgets_campaigns] FOREIGN KEY ([campaign_id]) REFERENCES [dbo].[campaigns] ([id])
);


