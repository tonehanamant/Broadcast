CREATE TABLE [dbo].[traffic_cumulative_cap_queues] (
    [proposal_id]  INT      NOT NULL,
    [employee_id]  INT      NOT NULL,
    [date_created] DATETIME NOT NULL,
    CONSTRAINT [PK_traffic_cumulative_cap_queues] PRIMARY KEY CLUSTERED ([proposal_id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_traffic_cumulative_cap_queues_employees] FOREIGN KEY ([employee_id]) REFERENCES [dbo].[employees] ([id]),
    CONSTRAINT [FK_traffic_cumulative_cap_queues_proposals] FOREIGN KEY ([proposal_id]) REFERENCES [dbo].[proposals] ([id])
);

