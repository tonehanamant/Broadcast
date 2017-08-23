CREATE TABLE [dbo].[traffic_breakdown_statistics] (
    [insertion_time] DATETIME       NOT NULL,
    [traffic_id]     INT            NOT NULL,
    [topography_id]  INT            NULL,
    [system_id]      INT            NULL,
    [total_seconds]  FLOAT (53)     NOT NULL,
    [message]        NVARCHAR (256) NULL,
    [employee_id]    INT            NULL,
    PRIMARY KEY CLUSTERED ([traffic_id] ASC, [insertion_time] ASC)
);

