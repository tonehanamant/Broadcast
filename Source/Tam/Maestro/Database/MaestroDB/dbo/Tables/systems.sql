CREATE TABLE [dbo].[systems] (
    [id]                                  INT          IDENTITY (1, 1) NOT NULL,
    [code]                                VARCHAR (15) NOT NULL,
    [name]                                VARCHAR (63) NOT NULL,
    [location]                            VARCHAR (63) NOT NULL,
    [spot_yield_weight]                   FLOAT (53)   CONSTRAINT [DF_systems_spot_yield_weight] DEFAULT ((1)) NOT NULL,
    [traffic_order_format]                INT          NOT NULL,
    [flag]                                TINYINT      CONSTRAINT [DF__systems__flag__1960B67E] DEFAULT ((1)) NULL,
    [active]                              BIT          NOT NULL,
    [effective_date]                      DATETIME     NOT NULL,
    [generate_traffic_alert_excel]        BIT          NOT NULL,
    [one_advertiser_per_traffic_alert]    BIT          NOT NULL,
    [cancel_recreate_order_traffic_alert] BIT          NOT NULL,
    [order_regeneration_traffic_alert]    BIT          NOT NULL,
    [custom_traffic_system]               BIT          NOT NULL,
    CONSTRAINT [PK_systems] PRIMARY KEY CLUSTERED ([id] ASC) WITH (FILLFACTOR = 90)
);








GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_system_code]
    ON [dbo].[systems]([code] ASC);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Contains the systems (also known as Selling Business Territories)', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'systems';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'Systems', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'systems';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'systems';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'systems', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'systems', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'systems', @level2type = N'COLUMN', @level2name = N'code';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'systems', @level2type = N'COLUMN', @level2name = N'code';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'systems', @level2type = N'COLUMN', @level2name = N'name';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'systems', @level2type = N'COLUMN', @level2name = N'name';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'systems', @level2type = N'COLUMN', @level2name = N'location';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'systems', @level2type = N'COLUMN', @level2name = N'location';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'systems', @level2type = N'COLUMN', @level2name = N'spot_yield_weight';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'systems', @level2type = N'COLUMN', @level2name = N'spot_yield_weight';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'systems', @level2type = N'COLUMN', @level2name = N'traffic_order_format';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'systems', @level2type = N'COLUMN', @level2name = N'traffic_order_format';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'systems', @level2type = N'COLUMN', @level2name = N'flag';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'systems', @level2type = N'COLUMN', @level2name = N'flag';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'systems', @level2type = N'COLUMN', @level2name = N'active';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'systems', @level2type = N'COLUMN', @level2name = N'active';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'systems', @level2type = N'COLUMN', @level2name = N'effective_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'systems', @level2type = N'COLUMN', @level2name = N'effective_date';

