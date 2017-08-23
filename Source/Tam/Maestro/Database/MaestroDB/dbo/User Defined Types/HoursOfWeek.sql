CREATE TYPE [dbo].[HoursOfWeek] AS TABLE (
    [component_daypart_id] INT     NOT NULL,
    [hour_of_week]         TINYINT NOT NULL,
    PRIMARY KEY CLUSTERED ([component_daypart_id] ASC, [hour_of_week] ASC));

