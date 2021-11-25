CREATE TABLE [dbo].[plan_version_custom_daypart_affiliate_restrictions]
	(
		[id] [int] PRIMARY KEY IDENTITY(1,1) NOT NULL,
		[plan_version_custom_daypart_id] [int] NOT NULL,
		[affiliate_id] [int] NOT NULL,
		CONSTRAINT [FK_plan_version_custom_daypart_affiliate_restrictions_affiliates] FOREIGN KEY ([affiliate_id]) REFERENCES [dbo].[affiliates]([ID]),
		CONSTRAINT [FK_plan_version_custom_daypart_affiliate_restrictions_plan_version_custom_dayparts] FOREIGN KEY([plan_version_custom_daypart_id])REFERENCES [dbo].[plan_version_custom_dayparts] ([id])
		
	)	
