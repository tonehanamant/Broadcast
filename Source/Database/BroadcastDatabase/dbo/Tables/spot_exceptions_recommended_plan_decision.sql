CREATE TABLE [dbo].[spot_exceptions_recommended_plan_decision]
(
	[id] INT IDENTITY(1,1) PRIMARY KEY,
	[spot_exceptions_recommended_plan_detail_id] INT NOT NULL,
	[username] VARCHAR(63) NOT NULL,
	[created_at] DATETIME NOT NULL,	
	[synced_by] VARCHAR(100) NULL, 
    [synced_at] DATETIME2 NULL, 
    CONSTRAINT [FK_spot_exceptions_recommended_plan_decision_spot_exceptions_recommended_plans_details] FOREIGN KEY ([spot_exceptions_recommended_plan_detail_id]) REFERENCES [dbo].[spot_exceptions_recommended_plan_details]([ID])
)