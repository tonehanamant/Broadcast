	CREATE PROCEDURE [dbo].[usp_affidavit_files_insert]
	(
		@id		Int		OUTPUT,
		@file_size		Int,
		@file_date		DateTime,
		@file_type		TinyInt,
		@checkin_date		DateTime,
		@load_date		DateTime,
		@status		Char(10),
		@original_filename		VarChar(255),
		@checkin_filename		VarChar(255),
		@load_duration		Int,
		@hash		Char(59),
		@total_checkin_invoice_count		Int,
		@total_checkin_affidavit_count		Int,
		@total_loaded_invoice_count		Int,
		@total_loaded_affidavit_count		Int,
		@total_duplicate_invoice_count		Int,
		@total_duplicate_affidavit_count		Int,
		@physical_file		VARBINARY(MAX),
		@forced_system_id		Int,
		@business_unit_id		TinyInt,
		@addressable_percentage		Float
	)
	AS
	BEGIN
		INSERT INTO affidavit_files
		(
			file_size,
			file_date,
			file_type,
			checkin_date,
			load_date,
			status,
			original_filename,
			checkin_filename,
			load_duration,
			hash,
			total_checkin_invoice_count,
			total_checkin_affidavit_count,
			total_loaded_invoice_count,
			total_loaded_affidavit_count,
			total_duplicate_invoice_count,
			total_duplicate_affidavit_count,
			physical_file,
			forced_system_id,
			business_unit_id,
			addressable_percentage
	
		)
		VALUES
		(
			@file_size,
			@file_date,
			@file_type,
			@checkin_date,
			@load_date,
			@status,
			@original_filename,
			@checkin_filename,
			@load_duration,
			@hash,
			@total_checkin_invoice_count,
			@total_checkin_affidavit_count,
			@total_loaded_invoice_count,
			@total_loaded_affidavit_count,
			@total_duplicate_invoice_count,
			@total_duplicate_affidavit_count,
			@physical_file,
			@forced_system_id,
			@business_unit_id,
			@addressable_percentage
		)

		SELECT
			@id = SCOPE_IDENTITY()
	END	
