CREATE TABLE [dbo].[frozen_topography_systems] (
    [media_month_id] SMALLINT NOT NULL,
    [topography_id]  INT      NOT NULL,
    [system_id]      INT      NOT NULL,
    [include]        BIT      NOT NULL,
    CONSTRAINT [PK_frozen_topography_systems] PRIMARY KEY CLUSTERED ([media_month_id] ASC, [topography_id] ASC, [system_id] ASC)
);

