CREATE TABLE [dbo].[frozen_system_group_systems] (
    [media_month_id]  SMALLINT NOT NULL,
    [system_group_id] INT      NOT NULL,
    [system_id]       INT      NOT NULL,
    CONSTRAINT [PK_frozen_system_group_systems] PRIMARY KEY CLUSTERED ([media_month_id] ASC, [system_group_id] ASC, [system_id] ASC) WITH (FILLFACTOR = 90)
);

