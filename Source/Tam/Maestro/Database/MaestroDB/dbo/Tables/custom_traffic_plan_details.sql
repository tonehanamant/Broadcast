CREATE TABLE [dbo].[custom_traffic_plan_details] (
    [network_id]               INT        NOT NULL,
    [custom_traffic_plan_id]   INT        NOT NULL,
    [traffic_factor]           FLOAT (53) NOT NULL,
    [spot_yield_weight_factor] FLOAT (53) NOT NULL,
    CONSTRAINT [PK_custom_traffic_plan_details] PRIMARY KEY CLUSTERED ([network_id] ASC, [custom_traffic_plan_id] ASC),
    CONSTRAINT [FK_custom_traffic_plan_details_custom_traffic_plans] FOREIGN KEY ([custom_traffic_plan_id]) REFERENCES [dbo].[custom_traffic_plans] ([id]),
    CONSTRAINT [FK_custom_traffic_plan_details_networks] FOREIGN KEY ([network_id]) REFERENCES [dbo].[networks] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'custom_traffic_plan_details';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'custom_traffic_plan_details';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'custom_traffic_plan_details';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'custom_traffic_plan_details', @level2type = N'COLUMN', @level2name = N'network_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'custom_traffic_plan_details', @level2type = N'COLUMN', @level2name = N'network_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'custom_traffic_plan_details', @level2type = N'COLUMN', @level2name = N'custom_traffic_plan_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'custom_traffic_plan_details', @level2type = N'COLUMN', @level2name = N'custom_traffic_plan_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'custom_traffic_plan_details', @level2type = N'COLUMN', @level2name = N'traffic_factor';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'custom_traffic_plan_details', @level2type = N'COLUMN', @level2name = N'traffic_factor';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'custom_traffic_plan_details', @level2type = N'COLUMN', @level2name = N'spot_yield_weight_factor';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'custom_traffic_plan_details', @level2type = N'COLUMN', @level2name = N'spot_yield_weight_factor';

