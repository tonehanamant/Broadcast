CREATE TABLE [dbo].[traffic_cap_monthly_override_rejections] (
    [traffic_id]       INT      NOT NULL,
    [employee_id]      INT      NOT NULL,
    [rejection_date]   DATETIME NOT NULL,
    [rejection_amount] MONEY    NOT NULL,
    CONSTRAINT [PK_traffic_cap_monthly_override_rejections] PRIMARY KEY CLUSTERED ([traffic_id] ASC) WITH (FILLFACTOR = 90)
);

