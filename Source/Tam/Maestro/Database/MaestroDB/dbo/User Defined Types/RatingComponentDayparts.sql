CREATE TYPE [dbo].[RatingComponentDayparts] AS TABLE (
    [request_id]           INT NOT NULL,
    [component_daypart_id] INT NOT NULL,
    [start_time]           INT NOT NULL,
    [end_time]             INT NOT NULL,
    [intersecting_hours]   INT NOT NULL,
    [weekends]             BIT NOT NULL,
    [weekdays]             BIT NOT NULL,
    PRIMARY KEY CLUSTERED ([request_id] ASC, [component_daypart_id] ASC));

