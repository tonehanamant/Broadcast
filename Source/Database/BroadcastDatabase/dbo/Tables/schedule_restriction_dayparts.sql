CREATE TABLE [dbo].[schedule_restriction_dayparts] (
    [schedule_id] INT NOT NULL,
    [daypart_id]  INT NOT NULL,
    CONSTRAINT [PK_schedule_restriction_dayparts] PRIMARY KEY CLUSTERED ([schedule_id] ASC, [daypart_id] ASC),
    CONSTRAINT [FK_schedule_restriction_dayparts_schedules] FOREIGN KEY ([schedule_id]) REFERENCES [dbo].[schedules] ([id]) ON DELETE CASCADE
);

