CREATE TABLE [dbo].[plan_version_custom_daypart_genre_restrictions]
(
	[id] [int] PRIMARY KEY IDENTITY(1,1) NOT NULL,
		[plan_version_custom_daypart_id] [int] NOT NULL,
		[genre_id] [int] NOT NULL,
		CONSTRAINT UQ_plan_version_custom_daypart_genre_restrictions UNIQUE (plan_version_custom_daypart_id,genre_id),
		CONSTRAINT [FK_plan_version_custom_daypart_genre_restrictions_genres] FOREIGN KEY([genre_id]) REFERENCES [dbo].[genres] ([id]),
		CONSTRAINT [FK_plan_version_custom_daypart_genre_restrictions_plan_version_custom_dayparts] FOREIGN KEY([plan_version_custom_daypart_id]) REFERENCES [dbo].[plan_version_custom_dayparts] ([id])
		
)
