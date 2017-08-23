CREATE TABLE [dbo].[affidavit_traffic_out] (
    [media_month_id]       SMALLINT     NOT NULL,
    [affidavit_id]         BIGINT       NOT NULL,
    [traffic_id]           INT          NULL,
    [traffic_detail_id]    INT          NULL,
    [traffic_order_id]     BIGINT       NULL,
    [bad_network]          BIT          NULL,
    [bad_system_network]   BIT          NULL,
    [bad_copy]             BIT          NULL,
    [bad_copy_flight]      BIT          NULL,
    [bad_air_time]         BIT          NULL,
    [bad_air_time_exact]   BIT          NULL,
    [bad_day_of_week]      BIT          NULL,
    [bad_flight]           BIT          NULL,
    [bad_flight_suspended] BIT          NULL,
    [bad_zone]             BIT          NULL,
    [bad_rate]             BIT          NULL,
    [calculated_rate]      MONEY        NULL,
    [status_code]          TINYINT      NOT NULL,
    [zone_relationship]    VARCHAR (63) NOT NULL,
    CONSTRAINT [PK_p_affidavit_traffic_out] PRIMARY KEY CLUSTERED ([media_month_id] ASC, [affidavit_id] ASC) ON [MediaMonthSmallintScheme] ([media_month_id])
);


GO
CREATE NONCLUSTERED INDEX [IX_affidavit_out_id]
    ON [dbo].[affidavit_traffic_out]([media_month_id] ASC, [status_code] ASC)
    INCLUDE([affidavit_id])
    ON [MediaMonthSmallintScheme] ([media_month_id]);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_traffic_out';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_traffic_out';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_traffic_out';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_traffic_out', @level2type = N'COLUMN', @level2name = N'media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_traffic_out', @level2type = N'COLUMN', @level2name = N'media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_traffic_out', @level2type = N'COLUMN', @level2name = N'affidavit_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_traffic_out', @level2type = N'COLUMN', @level2name = N'affidavit_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_traffic_out', @level2type = N'COLUMN', @level2name = N'traffic_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_traffic_out', @level2type = N'COLUMN', @level2name = N'traffic_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_traffic_out', @level2type = N'COLUMN', @level2name = N'traffic_detail_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_traffic_out', @level2type = N'COLUMN', @level2name = N'traffic_detail_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_traffic_out', @level2type = N'COLUMN', @level2name = N'traffic_order_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_traffic_out', @level2type = N'COLUMN', @level2name = N'traffic_order_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_traffic_out', @level2type = N'COLUMN', @level2name = N'bad_network';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_traffic_out', @level2type = N'COLUMN', @level2name = N'bad_network';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_traffic_out', @level2type = N'COLUMN', @level2name = N'bad_system_network';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_traffic_out', @level2type = N'COLUMN', @level2name = N'bad_system_network';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_traffic_out', @level2type = N'COLUMN', @level2name = N'bad_copy';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_traffic_out', @level2type = N'COLUMN', @level2name = N'bad_copy';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_traffic_out', @level2type = N'COLUMN', @level2name = N'bad_copy_flight';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_traffic_out', @level2type = N'COLUMN', @level2name = N'bad_copy_flight';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_traffic_out', @level2type = N'COLUMN', @level2name = N'bad_air_time';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_traffic_out', @level2type = N'COLUMN', @level2name = N'bad_air_time';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_traffic_out', @level2type = N'COLUMN', @level2name = N'bad_air_time_exact';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_traffic_out', @level2type = N'COLUMN', @level2name = N'bad_air_time_exact';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_traffic_out', @level2type = N'COLUMN', @level2name = N'bad_day_of_week';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_traffic_out', @level2type = N'COLUMN', @level2name = N'bad_day_of_week';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_traffic_out', @level2type = N'COLUMN', @level2name = N'bad_flight';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_traffic_out', @level2type = N'COLUMN', @level2name = N'bad_flight';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_traffic_out', @level2type = N'COLUMN', @level2name = N'bad_flight_suspended';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_traffic_out', @level2type = N'COLUMN', @level2name = N'bad_flight_suspended';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_traffic_out', @level2type = N'COLUMN', @level2name = N'bad_zone';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_traffic_out', @level2type = N'COLUMN', @level2name = N'bad_zone';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_traffic_out', @level2type = N'COLUMN', @level2name = N'bad_rate';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_traffic_out', @level2type = N'COLUMN', @level2name = N'bad_rate';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_traffic_out', @level2type = N'COLUMN', @level2name = N'calculated_rate';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_traffic_out', @level2type = N'COLUMN', @level2name = N'calculated_rate';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_traffic_out', @level2type = N'COLUMN', @level2name = N'status_code';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_traffic_out', @level2type = N'COLUMN', @level2name = N'status_code';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_traffic_out', @level2type = N'COLUMN', @level2name = N'zone_relationship';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_traffic_out', @level2type = N'COLUMN', @level2name = N'zone_relationship';

