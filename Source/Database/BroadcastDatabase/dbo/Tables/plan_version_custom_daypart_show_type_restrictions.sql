CREATE TABLE [dbo].[plan_version_custom_daypart_show_type_restrictions]
(
	[id] [int] PRIMARY KEY IDENTITY(1,1) NOT NULL,
		[plan_version_custom_daypart_id] [int] NOT NULL,
		[show_type_id] [int] NOT NULL,
		CONSTRAINT UQ_plan_version_custom_daypart_show_type_restrictions UNIQUE (plan_version_custom_daypart_id,show_type_id),
		CONSTRAINT [FK_plan_version_custom_daypart_show_type_restrictions_plan_version_custom_dayparts] FOREIGN KEY([plan_version_custom_daypart_id])REFERENCES [dbo].[plan_version_custom_dayparts] ([id]),
		CONSTRAINT [FK_plan_version_custom_daypart_show_type_restrictions_show_types] FOREIGN KEY([show_type_id])REFERENCES [dbo].[show_types] ([id])
		
)
