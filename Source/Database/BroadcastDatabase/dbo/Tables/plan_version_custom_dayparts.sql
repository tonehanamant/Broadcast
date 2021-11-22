CREATE TABLE [dbo].[plan_version_custom_dayparts]
	(
		[id] INT NOT NULL PRIMARY KEY IDENTITY (1, 1), 
		[custom_daypart_organization_id] INT NOT NULL, 
		[custom_daypart_name] NVARCHAR(100)  NOT NULL,
		[start_time_seconds] [int] NOT NULL,
		[end_time_seconds] [int] NOT NULL,
		[weighting_goal_percent] [float] NULL,
		[daypart_type] [int] NOT NULL,
		[is_start_time_modified] [bit] NOT NULL,
		[is_end_time_modified] [bit] NOT NULL,
		[plan_version_id] [int] NOT NULL,
		[show_type_restrictions_contain_type] [int] NULL,
		[genre_restrictions_contain_type] [int] NULL,
		[program_restrictions_contain_type] [int] NULL,
		[affiliate_restrictions_contain_type] [int] NULL,
		[weekdays_weighting] [float] NULL,
		[weekend_weighting] [float] NULL,
		CONSTRAINT [FK_plan_version_custom_dayparts_custom_daypart_organizations] FOREIGN KEY ([custom_daypart_organization_id]) REFERENCES [dbo].[custom_daypart_organizations]([ID])
	)	