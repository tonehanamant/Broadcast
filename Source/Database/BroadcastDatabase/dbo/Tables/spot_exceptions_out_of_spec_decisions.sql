CREATE TABLE [dbo].[spot_exceptions_out_of_spec_decisions]
(
	id INT IDENTITY(1,1) PRIMARY KEY,
	spot_exceptions_out_of_spec_id INT NOT NULL,
	accepted_as_in_spec BIT NOT NULL,
	decision_notes NVARCHAR(1024) NULL,
	username VARCHAR(63) NOT NULL,
	created_at DATETIME NOT NULL,
	[synced_by] VARCHAR(100) NULL, 
    [synced_at] DATETIME2 NULL, 
    CONSTRAINT [FK_spot_exceptions_out_of_spec_decisions_spot_exceptions_out_of_specs] FOREIGN KEY ([spot_exceptions_out_of_spec_id]) REFERENCES [dbo].[spot_exceptions_out_of_specs]([ID])
)
