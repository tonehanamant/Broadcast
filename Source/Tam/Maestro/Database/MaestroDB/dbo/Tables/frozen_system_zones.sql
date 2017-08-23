CREATE TABLE [dbo].[frozen_system_zones] (
    [media_month_id] SMALLINT     NOT NULL,
    [zone_id]        INT          NOT NULL,
    [system_id]      INT          NOT NULL,
    [type]           VARCHAR (15) NOT NULL,
    CONSTRAINT [PK_frozen_system_zones] PRIMARY KEY CLUSTERED ([media_month_id] ASC, [zone_id] ASC, [system_id] ASC, [type] ASC)
);

