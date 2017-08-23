CREATE TABLE [dbo].[traffic_cap_monthly_override_approvals] (
    [traffic_id]      INT      NOT NULL,
    [employee_id]     INT      NOT NULL,
    [approval_date]   DATETIME NOT NULL,
    [approval_amount] MONEY    NOT NULL,
    CONSTRAINT [PK_traffic_cap_monthly_override_approvals] PRIMARY KEY CLUSTERED ([traffic_id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_traffic_cap_monthly_override_approvals_employees] FOREIGN KEY ([employee_id]) REFERENCES [dbo].[employees] ([id]),
    CONSTRAINT [FK_traffic_cap_monthly_override_approvals_traffic] FOREIGN KEY ([traffic_id]) REFERENCES [dbo].[traffic] ([id])
);

