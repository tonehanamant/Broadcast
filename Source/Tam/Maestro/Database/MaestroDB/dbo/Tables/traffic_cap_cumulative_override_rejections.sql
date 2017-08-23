CREATE TABLE [dbo].[traffic_cap_cumulative_override_rejections] (
    [proposal_id]      INT      NOT NULL,
    [employee_id]      INT      NOT NULL,
    [rejection_date]   DATETIME NOT NULL,
    [rejection_amount] MONEY    NOT NULL,
    CONSTRAINT [PK_traffic_cap_cumulative_override_rejections] PRIMARY KEY CLUSTERED ([proposal_id] ASC) WITH (FILLFACTOR = 90)
);

