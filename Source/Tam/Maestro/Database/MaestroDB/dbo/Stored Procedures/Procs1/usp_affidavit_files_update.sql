	-- =============================================
	-- Author:		CRUD Creator
	-- Create date: 02/18/2015 01:43:28 PM
	-- Description:	Auto-generated method to update a affidavit_files record.
	-- =============================================
	CREATE PROCEDURE usp_affidavit_files_update
		@id INT,
		@file_size INT,
		@file_date DATETIME,
		@file_type TINYINT,
		@checkin_date DATETIME,
		@load_date DATETIME,
		@status CHAR(10),
		@original_filename VARCHAR(255),
		@checkin_filename VARCHAR(255),
		@load_duration INT,
		@hash CHAR(59),
		@total_checkin_invoice_count INT,
		@total_checkin_affidavit_count INT,
		@total_loaded_invoice_count INT,
		@total_loaded_affidavit_count INT,
		@total_duplicate_invoice_count INT,
		@total_duplicate_affidavit_count INT,
		@physical_file VARBINARY(MAX),
		@forced_system_id INT,
		@business_unit_id TINYINT,
		@addressable_percentage FLOAT
	AS
	BEGIN
		UPDATE
			[dbo].[affidavit_files]
		SET
			[file_size]=@file_size,
			[file_date]=@file_date,
			[file_type]=@file_type,
			[checkin_date]=@checkin_date,
			[load_date]=@load_date,
			[status]=@status,
			[original_filename]=@original_filename,
			[checkin_filename]=@checkin_filename,
			[load_duration]=@load_duration,
			[hash]=@hash,
			[total_checkin_invoice_count]=@total_checkin_invoice_count,
			[total_checkin_affidavit_count]=@total_checkin_affidavit_count,
			[total_loaded_invoice_count]=@total_loaded_invoice_count,
			[total_loaded_affidavit_count]=@total_loaded_affidavit_count,
			[total_duplicate_invoice_count]=@total_duplicate_invoice_count,
			[total_duplicate_affidavit_count]=@total_duplicate_affidavit_count,
			[physical_file]=@physical_file,
			[forced_system_id]=@forced_system_id,
			[business_unit_id]=@business_unit_id,
			[addressable_percentage]=@addressable_percentage
		WHERE
			[id]=@id
	END
