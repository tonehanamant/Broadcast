CREATE TABLE [dbo].[spot_exceptions_recommended_plan_decision]
(
	[id] INT IDENTITY(1,1) PRIMARY KEY,
	[selected_details_id] INT NOT NULL,
	[username] VARCHAR(63) NOT NULL,
	[created_at] DATETIME NOT NULL,	
	CONSTRAINT [FK_spot_exceptions_recommended_plan_decision_spot_exceptions_recommended_plans_details] FOREIGN KEY ([selected_details_id]) REFERENCES [dbo].[spot_exceptions_recommended_plan_details]([ID])
)