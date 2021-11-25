CREATE TABLE [dbo].[plan_version_custom_daypart_program_restrictions]
(
	[id] [int] PRIMARY KEY IDENTITY(1,1) NOT NULL,
		[plan_version_custom_daypart_id] [int] NOT NULL,
		[program_name] [varchar](255) NULL,
		[genre_id] [int] NULL,
		[content_rating] [varchar](15) NULL,
		CONSTRAINT UC_plan_version_custom_daypart_program_restrictions UNIQUE (plan_version_custom_daypart_id,[program_name],genre_id),
		CONSTRAINT [FK_plan_version_custom_daypart_program_restrictions_plan_version_custom_dayparts] FOREIGN KEY([plan_version_custom_daypart_id])
REFERENCES [dbo].[plan_version_custom_dayparts] ([id])
)
