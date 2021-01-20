CREATE TABLE [dbo].[schedule_restriction_markets] (
    [markets_market_code] SMALLINT NOT NULL,
    [schedules_id]        INT      NOT NULL,
    CONSTRAINT [PK_schedule_restriction_markets] PRIMARY KEY CLUSTERED ([markets_market_code] ASC, [schedules_id] ASC),
    CONSTRAINT [FK_schedule_restriction_markets_market] FOREIGN KEY ([markets_market_code]) REFERENCES [dbo].[markets] ([market_code]),
    CONSTRAINT [FK_schedule_restriction_markets_schedule] FOREIGN KEY ([schedules_id]) REFERENCES [dbo].[schedules] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_schedule_restriction_markets_schedule]
    ON [dbo].[schedule_restriction_markets]([schedules_id] ASC);

