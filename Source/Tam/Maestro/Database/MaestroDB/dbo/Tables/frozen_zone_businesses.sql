CREATE TABLE [dbo].[frozen_zone_businesses] (
    [media_month_id] SMALLINT     NOT NULL,
    [business_id]    INT          NOT NULL,
    [type]           VARCHAR (15) NOT NULL,
    [zone_id]        INT          NOT NULL,
    CONSTRAINT [PK_frozen_zones_business] PRIMARY KEY CLUSTERED ([media_month_id] ASC, [business_id] ASC, [type] ASC, [zone_id] ASC) WITH (FILLFACTOR = 90)
);



