CREATE TABLE [dbo].[frozen_topography_system_groups] (
    [media_month_id]  SMALLINT NOT NULL,
    [topography_id]   INT      NOT NULL,
    [system_group_id] INT      NOT NULL,
    [include]         BIT      NOT NULL,
    CONSTRAINT [PK_frozen_topography_system_groups] PRIMARY KEY CLUSTERED ([media_month_id] ASC, [topography_id] ASC, [system_group_id] ASC) WITH (FILLFACTOR = 90)
);

