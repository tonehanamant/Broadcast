CREATE TABLE [dbo].[plan_iscis]
(
	[id] INT NOT NULL PRIMARY KEY IDENTITY (1, 1), 
	[plan_id] INT NOT NULL, 
	[isci] VARCHAR(50) NOT NULL, 
	[created_at] DATETIME2 NOT NULL,
	[created_by] VARCHAR(100) NOT NULL,
	[deleted_at] DATETIME2 NULL,
	[deleted_by] VARCHAR(100) NULL,
	CONSTRAINT [FK_plan_iscis_plans] FOREIGN KEY ([plan_id]) REFERENCES [dbo].[plans] ([id])
);
