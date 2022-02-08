CREATE TABLE [dbo].[plan_iscis]
(
	[id] INT NOT NULL PRIMARY KEY IDENTITY (1, 1), 
	[plan_id] INT NOT NULL, 
	[isci] VARCHAR(50) NOT NULL, 
	[created_at] DATETIME2 NOT NULL,
	[created_by] VARCHAR(100) NOT NULL,
	[deleted_at] DATETIME2 NULL,
	[deleted_by] VARCHAR(100) NULL,
	[flight_start_date] DATE NOT NULL, 
    [flight_end_date] DATE NOT NULL,
	[modified_at] [datetime2](7) NOT NULL,
	[modified_by] [varchar](100) NOT NULL,
    CONSTRAINT [FK_plan_iscis_plans] FOREIGN KEY ([plan_id]) REFERENCES [dbo].[plans] ([id])
);

GO

CREATE UNIQUE NONCLUSTERED INDEX [UX_plan_iscis_plan_id_isci] ON [dbo].[plan_iscis]
(
	[plan_id] ASC,
	[isci] ASC,
	[deleted_at] ASC,
	[flight_start_date] ASC,
	[flight_end_date] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
