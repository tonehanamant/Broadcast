

/****** Object:  StoredProcedure [dbo].[usp_broadcast_affidavit_files_update]    Script Date: 04/03/2014 08:16:55 ******/
CREATE PROCEDURE usp_broadcast_affidavit_files_update
(
	@id		Int,
	@file_data		VARBINARY(MAX),
	@file_size		BigInt,
	@file_name		VarChar(255),
	@num_lines		Int,
	@start_date		DateTime,
	@end_date		DateTime,
	@hash		VarChar(63),
	@load_duration		Float,
	@time_started		DateTime,
	@time_completed		DateTime
)
AS
	UPDATE dbo.broadcast_affidavit_files SET
		file_data = @file_data,
		file_size = @file_size,
		file_name = @file_name,
		num_lines = @num_lines,
		start_date = @start_date,
		end_date = @end_date,
		hash = @hash,
		load_duration = @load_duration,
		time_started = @time_started,
		time_completed = @time_completed
	WHERE
		id = @id
