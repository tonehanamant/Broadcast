CREATE TABLE [dbo].[traffic_custom_traffic_plans] (
    [traffic_id]             INT NOT NULL,
    [custom_traffic_plan_id] INT NOT NULL,
    CONSTRAINT [PK_traffic_custom_traffic_plans] PRIMARY KEY CLUSTERED ([traffic_id] ASC, [custom_traffic_plan_id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_traffic_custom_traffic_plans_custom_traffic_plans] FOREIGN KEY ([custom_traffic_plan_id]) REFERENCES [dbo].[custom_traffic_plans] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_custom_traffic_plans';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_custom_traffic_plans';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_custom_traffic_plans';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_custom_traffic_plans', @level2type = N'COLUMN', @level2name = N'traffic_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_custom_traffic_plans', @level2type = N'COLUMN', @level2name = N'traffic_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_custom_traffic_plans', @level2type = N'COLUMN', @level2name = N'custom_traffic_plan_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_custom_traffic_plans', @level2type = N'COLUMN', @level2name = N'custom_traffic_plan_id';

