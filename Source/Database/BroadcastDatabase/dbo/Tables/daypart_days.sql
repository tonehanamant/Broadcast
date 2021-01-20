CREATE TABLE [dbo].[daypart_days] (
    [dayparts_id] INT NOT NULL,
    [days_id]     INT NOT NULL,
    CONSTRAINT [PK_daypart_days] PRIMARY KEY CLUSTERED ([dayparts_id] ASC, [days_id] ASC),
    CONSTRAINT [FK_daypart_days_day] FOREIGN KEY ([days_id]) REFERENCES [dbo].[days] ([id]),
    CONSTRAINT [FK_daypart_days_daypart] FOREIGN KEY ([dayparts_id]) REFERENCES [dbo].[dayparts] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_daypart_days_day]
    ON [dbo].[daypart_days]([days_id] ASC);

