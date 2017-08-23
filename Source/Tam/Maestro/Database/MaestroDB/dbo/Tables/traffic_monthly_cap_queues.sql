CREATE TABLE [dbo].[traffic_monthly_cap_queues] (
    [traffic_id]   INT      NOT NULL,
    [employee_id]  INT      NOT NULL,
    [date_created] DATETIME NOT NULL,
    CONSTRAINT [PK_traffic_monthly_cap_queues] PRIMARY KEY CLUSTERED ([traffic_id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_traffic_monthly_cap_queues_employees] FOREIGN KEY ([employee_id]) REFERENCES [dbo].[employees] ([id]),
    CONSTRAINT [FK_traffic_monthly_cap_queues_traffic] FOREIGN KEY ([traffic_id]) REFERENCES [dbo].[traffic] ([id])
);

