CREATE TABLE [dbo].[spot_exceptions_recommended_plan_details]
(
	[id] INT IDENTITY(1,1) PRIMARY KEY,
	[spot_exceptions_recommended_plan_id] INT NOT NULL,-- FK (spot_exceptions_recommended_plans.id),
	[recommended_plan_id] INT NOT NULL,-- FK(plans.id),
	[metric_percent] FLOAT NOT NULL,
	CONSTRAINT [FK_spot_exceptions_recommended_plan_details_spot_exceptions_recommended_plans] FOREIGN KEY ([spot_exceptions_recommended_plan_id]) REFERENCES [dbo].[spot_exceptions_recommended_plans]([ID]),
	CONSTRAINT [FK_spot_exceptions_recommended_plan_details_plans] FOREIGN KEY ([recommended_plan_id]) REFERENCES [dbo].[plans]([ID]),	

)
