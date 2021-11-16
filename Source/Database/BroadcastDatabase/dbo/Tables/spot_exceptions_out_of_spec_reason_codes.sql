CREATE TABLE [dbo].[spot_exceptions_out_of_spec_reason_codes](
		[id] [int] NOT NULL PRIMARY KEY IDENTITY(1,1),
		[reason_code] [int] NOT NULL,
		[reason] [nvarchar](200) NOT NULL,
		[label] [nvarchar](200) NULL
	)