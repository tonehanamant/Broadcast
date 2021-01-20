CREATE TABLE [dbo].[plan_version_flight_days] (
    [id]              INT IDENTITY (1, 1) NOT NULL,
    [plan_version_id] INT NOT NULL,
    [day_id]          INT NOT NULL,
    CONSTRAINT [PK_plan_version_flight_days] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_plan_version_flight_days_days] FOREIGN KEY ([day_id]) REFERENCES [dbo].[days] ([id]),
    CONSTRAINT [FK_plan_version_flight_days_plan_versions] FOREIGN KEY ([plan_version_id]) REFERENCES [dbo].[plan_versions] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_plan_version_flight_days_plan_versions]
    ON [dbo].[plan_version_flight_days]([plan_version_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_plan_version_flight_days_days]
    ON [dbo].[plan_version_flight_days]([day_id] ASC);

