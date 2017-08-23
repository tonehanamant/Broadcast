CREATE TYPE [dbo].[InventoryRequestTable] AS TABLE (
    [media_month_id]         SMALLINT NOT NULL,
    [media_week_id]          INT      NOT NULL,
    [network_id]             INT      NOT NULL,
    [daypart_id]             INT      NOT NULL,
    [hh_eq_cpm]              MONEY    NOT NULL,
    [contracted_subscribers] BIGINT   NOT NULL,
    PRIMARY KEY CLUSTERED ([media_month_id] ASC, [media_week_id] ASC, [network_id] ASC, [daypart_id] ASC, [hh_eq_cpm] ASC));

