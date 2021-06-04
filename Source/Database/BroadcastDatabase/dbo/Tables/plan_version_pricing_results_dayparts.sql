CREATE TABLE [dbo].[plan_version_pricing_results_dayparts]
(
	[id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [plan_version_pricing_result_id] INT NOT NULL, 
    [standard_daypart_id] INT NOT NULL, 
    [calculated_vpvh] FLOAT NOT NULL,
     CONSTRAINT [FK_plan_version_pricing_results_dayparts_plan_version_pricing_results] FOREIGN KEY ([plan_version_pricing_result_id]) REFERENCES [dbo].[plan_version_pricing_results],
     CONSTRAINT [FK_plan_version_pricing_results_dayparts_standard_dayparts] FOREIGN KEY ([standard_daypart_id]) REFERENCES [dbo].[standard_dayparts]
)
