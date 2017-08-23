CREATE TABLE [wb].[wb_schedules] (
    [id]             INT           IDENTITY (1, 1) NOT NULL,
    [region_code]    VARCHAR (127) NOT NULL,
    [media_week_id]  INT           NOT NULL,
    [start_time]     INT           NULL,
    [end_time]       INT           NULL,
    [day_id]         INT           NULL,
    [wb_agency_id]   INT           NULL,
    [order_title]    VARCHAR (127) NULL,
    [status]         VARCHAR (127) NULL,
    [net_rate]       MONEY         NULL,
    [agency_percent] DECIMAL (18)  NULL,
    [gross_rate]     MONEY         NULL,
    [tape_id]        VARCHAR (127) NULL,
    [phone_number]   VARCHAR (127) NULL,
    [sales_person]   VARCHAR (127) NULL,
    [rate_card_rate] MONEY         NULL,
    CONSTRAINT [PK_wb_schedules] PRIMARY KEY CLUSTERED ([id] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [NK_wb_schedules]
    ON [wb].[wb_schedules]([region_code] ASC, [media_week_id] ASC, [day_id] ASC, [start_time] ASC);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'wb', @level1type = N'TABLE', @level1name = N'wb_schedules';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'wb', @level1type = N'TABLE', @level1name = N'wb_schedules';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'wb', @level1type = N'TABLE', @level1name = N'wb_schedules';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'wb', @level1type = N'TABLE', @level1name = N'wb_schedules', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'wb', @level1type = N'TABLE', @level1name = N'wb_schedules', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'wb', @level1type = N'TABLE', @level1name = N'wb_schedules', @level2type = N'COLUMN', @level2name = N'region_code';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'wb', @level1type = N'TABLE', @level1name = N'wb_schedules', @level2type = N'COLUMN', @level2name = N'region_code';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'wb', @level1type = N'TABLE', @level1name = N'wb_schedules', @level2type = N'COLUMN', @level2name = N'media_week_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'wb', @level1type = N'TABLE', @level1name = N'wb_schedules', @level2type = N'COLUMN', @level2name = N'media_week_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'wb', @level1type = N'TABLE', @level1name = N'wb_schedules', @level2type = N'COLUMN', @level2name = N'start_time';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'wb', @level1type = N'TABLE', @level1name = N'wb_schedules', @level2type = N'COLUMN', @level2name = N'start_time';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'wb', @level1type = N'TABLE', @level1name = N'wb_schedules', @level2type = N'COLUMN', @level2name = N'end_time';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'wb', @level1type = N'TABLE', @level1name = N'wb_schedules', @level2type = N'COLUMN', @level2name = N'end_time';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'wb', @level1type = N'TABLE', @level1name = N'wb_schedules', @level2type = N'COLUMN', @level2name = N'day_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'wb', @level1type = N'TABLE', @level1name = N'wb_schedules', @level2type = N'COLUMN', @level2name = N'day_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'wb', @level1type = N'TABLE', @level1name = N'wb_schedules', @level2type = N'COLUMN', @level2name = N'wb_agency_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'wb', @level1type = N'TABLE', @level1name = N'wb_schedules', @level2type = N'COLUMN', @level2name = N'wb_agency_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'wb', @level1type = N'TABLE', @level1name = N'wb_schedules', @level2type = N'COLUMN', @level2name = N'order_title';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'wb', @level1type = N'TABLE', @level1name = N'wb_schedules', @level2type = N'COLUMN', @level2name = N'order_title';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'wb', @level1type = N'TABLE', @level1name = N'wb_schedules', @level2type = N'COLUMN', @level2name = N'status';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'wb', @level1type = N'TABLE', @level1name = N'wb_schedules', @level2type = N'COLUMN', @level2name = N'status';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'wb', @level1type = N'TABLE', @level1name = N'wb_schedules', @level2type = N'COLUMN', @level2name = N'net_rate';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'wb', @level1type = N'TABLE', @level1name = N'wb_schedules', @level2type = N'COLUMN', @level2name = N'net_rate';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'wb', @level1type = N'TABLE', @level1name = N'wb_schedules', @level2type = N'COLUMN', @level2name = N'agency_percent';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'wb', @level1type = N'TABLE', @level1name = N'wb_schedules', @level2type = N'COLUMN', @level2name = N'agency_percent';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'wb', @level1type = N'TABLE', @level1name = N'wb_schedules', @level2type = N'COLUMN', @level2name = N'gross_rate';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'wb', @level1type = N'TABLE', @level1name = N'wb_schedules', @level2type = N'COLUMN', @level2name = N'gross_rate';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'wb', @level1type = N'TABLE', @level1name = N'wb_schedules', @level2type = N'COLUMN', @level2name = N'tape_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'wb', @level1type = N'TABLE', @level1name = N'wb_schedules', @level2type = N'COLUMN', @level2name = N'tape_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'wb', @level1type = N'TABLE', @level1name = N'wb_schedules', @level2type = N'COLUMN', @level2name = N'phone_number';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'wb', @level1type = N'TABLE', @level1name = N'wb_schedules', @level2type = N'COLUMN', @level2name = N'phone_number';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'wb', @level1type = N'TABLE', @level1name = N'wb_schedules', @level2type = N'COLUMN', @level2name = N'sales_person';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'wb', @level1type = N'TABLE', @level1name = N'wb_schedules', @level2type = N'COLUMN', @level2name = N'sales_person';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'wb', @level1type = N'TABLE', @level1name = N'wb_schedules', @level2type = N'COLUMN', @level2name = N'rate_card_rate';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'wb', @level1type = N'TABLE', @level1name = N'wb_schedules', @level2type = N'COLUMN', @level2name = N'rate_card_rate';

