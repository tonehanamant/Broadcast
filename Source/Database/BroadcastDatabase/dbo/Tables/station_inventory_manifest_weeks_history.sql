CREATE TABLE [dbo].[station_inventory_manifest_weeks_history] (
    [id]                            INT      NOT NULL,
    [station_inventory_manifest_id] INT      NOT NULL,
    [media_week_id]                 INT      NOT NULL,
    [spots]                         INT      NOT NULL,
    [start_date]                    DATETIME NOT NULL,
    [end_date]                      DATETIME NOT NULL,
    [sys_start_date]                DATETIME NOT NULL,
    [sys_end_date]                  DATETIME NOT NULL,
    CONSTRAINT [PK_station_inventory_manifest_weeks_history] PRIMARY KEY CLUSTERED ([id] ASC, [station_inventory_manifest_id] ASC, [media_week_id] ASC, [spots] ASC, [start_date] ASC, [end_date] ASC, [sys_start_date] ASC, [sys_end_date] ASC)
);

