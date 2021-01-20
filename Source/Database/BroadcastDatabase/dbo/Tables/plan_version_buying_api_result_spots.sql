CREATE TABLE [dbo].[plan_version_buying_api_result_spots] (
    [id]                                 INT        IDENTITY (1, 1) NOT NULL,
    [plan_version_buying_api_results_id] INT        NOT NULL,
    [station_inventory_manifest_id]      INT        NOT NULL,
    [inventory_media_week_id]            INT        NOT NULL,
    [impressions30sec]                   FLOAT (53) NOT NULL,
    [contract_media_week_id]             INT        NOT NULL,
    [standard_daypart_id]                INT        NOT NULL,
    CONSTRAINT [PK_plan_version_buying_api_result_spots] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_plan_version_buying_api_result_spots_contract_media_week] FOREIGN KEY ([contract_media_week_id]) REFERENCES [dbo].[media_weeks] ([id]),
    CONSTRAINT [FK_plan_version_buying_api_result_spots_daypart_defaults1] FOREIGN KEY ([standard_daypart_id]) REFERENCES [dbo].[standard_dayparts] ([id]),
    CONSTRAINT [FK_plan_version_buying_api_result_spots_inventory_media_week] FOREIGN KEY ([inventory_media_week_id]) REFERENCES [dbo].[media_weeks] ([id]) ON DELETE CASCADE,
    CONSTRAINT [FK_plan_version_buying_api_result_spots_plan_version_buying_api_results] FOREIGN KEY ([plan_version_buying_api_results_id]) REFERENCES [dbo].[plan_version_buying_api_results] ([id]) ON DELETE CASCADE,
    CONSTRAINT [FK_plan_version_buying_api_result_spots_station_inventory_manifest] FOREIGN KEY ([station_inventory_manifest_id]) REFERENCES [dbo].[station_inventory_manifest] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_plan_version_buying_api_result_spots_daypart_defaults1]
    ON [dbo].[plan_version_buying_api_result_spots]([standard_daypart_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_plan_version_buying_api_result_spots_station_inventory_manifest]
    ON [dbo].[plan_version_buying_api_result_spots]([station_inventory_manifest_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_plan_version_buying_api_result_spots_plan_version_buying_api_results]
    ON [dbo].[plan_version_buying_api_result_spots]([plan_version_buying_api_results_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_plan_version_buying_api_result_spots_inventory_media_week]
    ON [dbo].[plan_version_buying_api_result_spots]([inventory_media_week_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_plan_version_buying_api_result_spots_contract_media_week]
    ON [dbo].[plan_version_buying_api_result_spots]([contract_media_week_id] ASC);

