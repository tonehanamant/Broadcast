CREATE TABLE [dbo].[plan_version_daypart_flight_days] (
    [id]                           INT IDENTITY (1, 1) NOT NULL,
    [plan_version_daypart_goal_id] INT NOT NULL,
    [day_id]                       INT NOT NULL,
    CONSTRAINT [FK_plan_version_daypart_flight_days_days] FOREIGN KEY ([day_id]) REFERENCES [dbo].[days] ([id]),
    CONSTRAINT [FK_plan_version_daypart_flight_days_plan_version_daypart_goals] FOREIGN KEY ([plan_version_daypart_goal_id]) REFERENCES [dbo].[plan_version_daypart_goals] ([id]) ON DELETE CASCADE
);

