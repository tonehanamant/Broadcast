CREATE TABLE [dbo].[cmw_traffic_detail_days] (
    [cmw_traffic_details_id] INT NOT NULL,
    [day_id]                 INT NOT NULL,
    [units]                  INT NOT NULL,
    [is_max]                 BIT NOT NULL,
    CONSTRAINT [PK_cmw_traffic_detail_days] PRIMARY KEY CLUSTERED ([cmw_traffic_details_id] ASC, [day_id] ASC),
    CONSTRAINT [FK_cmw_traffic_detail_days_cmw_traffic_details] FOREIGN KEY ([cmw_traffic_details_id]) REFERENCES [dbo].[cmw_traffic_details] ([id]),
    CONSTRAINT [FK_cmw_traffic_detail_days_days] FOREIGN KEY ([day_id]) REFERENCES [dbo].[days] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'cmw_traffic_detail_days';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'cmw_traffic_detail_days';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'cmw_traffic_detail_days';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'cmw_traffic_detail_days', @level2type = N'COLUMN', @level2name = N'cmw_traffic_details_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'cmw_traffic_detail_days', @level2type = N'COLUMN', @level2name = N'cmw_traffic_details_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'cmw_traffic_detail_days', @level2type = N'COLUMN', @level2name = N'day_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'cmw_traffic_detail_days', @level2type = N'COLUMN', @level2name = N'day_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'cmw_traffic_detail_days', @level2type = N'COLUMN', @level2name = N'units';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'cmw_traffic_detail_days', @level2type = N'COLUMN', @level2name = N'units';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'cmw_traffic_detail_days', @level2type = N'COLUMN', @level2name = N'is_max';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'cmw_traffic_detail_days', @level2type = N'COLUMN', @level2name = N'is_max';

