CREATE TABLE [dbo].[schedule_audiences] (
    [id]          INT IDENTITY (1, 1) NOT NULL,
    [schedule_id] INT NOT NULL,
    [audience_id] INT NOT NULL,
    [population]  INT NOT NULL,
    [rank]        INT NOT NULL,
    CONSTRAINT [PK_schedule_audiences] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_schedule_audiences_audiences] FOREIGN KEY ([audience_id]) REFERENCES [dbo].[audiences] ([id]),
    CONSTRAINT [FK_schedule_audiences_schedule] FOREIGN KEY ([schedule_id]) REFERENCES [dbo].[schedules] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_schedule_audiences_schedule]
    ON [dbo].[schedule_audiences]([schedule_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_schedule_audiences_audiences]
    ON [dbo].[schedule_audiences]([audience_id] ASC);

