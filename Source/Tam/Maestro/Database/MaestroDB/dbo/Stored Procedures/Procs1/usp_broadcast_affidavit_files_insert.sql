

/****** Object:  StoredProcedure [dbo].[usp_broadcast_affidavit_files_insert]    Script Date: 04/03/2014 08:16:55 ******/
CREATE PROCEDURE usp_broadcast_affidavit_files_insert
	@id INT OUTPUT,
	@file_data VARBINARY(MAX),
	@file_size BIGINT,
	@file_name VARCHAR(255),
	@num_lines INT,
	@start_date DATETIME,
	@end_date DATETIME,
	@hash VARCHAR(63),
	@load_duration FLOAT,
	@time_started DATETIME,
	@time_completed DATETIME
AS
BEGIN
	INSERT INTO [dbo].[broadcast_affidavit_files]
	(
		[file_data],
		[file_size],
		[file_name],
		[num_lines],
		[start_date],
		[end_date],
		[hash],
		[load_duration],
		[time_started],
		[time_completed]
	)
	VALUES
	(
		@file_data,
		@file_size,
		@file_name,
		@num_lines,
		@start_date,
		@end_date,
		@hash,
		@load_duration,
		@time_started,
		@time_completed
	)

	SELECT
		@id = SCOPE_IDENTITY()
END
