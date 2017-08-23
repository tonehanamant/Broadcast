CREATE PROCEDURE [dbo].[usp_ACS_GetRawAffidavitFile_ByHash]
	@hash VARCHAR(63)
AS
BEGIN
	SELECT 
		af.id, 
		af.file_size, 
		af.file_date, 
		af.file_type, 
		af.checkin_date, 
		af.load_date, 
		af.status, 
		af.original_filename, 
		af.checkin_filename, 
		af.load_duration, 
		af.hash, 
		af.total_checkin_invoice_count, 
		af.total_checkin_affidavit_count, 
		af.total_loaded_invoice_count, 
		af.total_loaded_affidavit_count, 
		af.total_duplicate_invoice_count, 
		af.total_duplicate_affidavit_count,
		af.forced_system_id,
		s.code,
		af.business_unit_id
	FROM
		affidavit_files	af				(NOLOCK)
		LEFT JOIN uvw_system_universe s	(NOLOCK) ON s.system_id=af.forced_system_id AND (start_date<=af.checkin_date AND (s.end_date>=af.checkin_date OR s.end_date IS NULL))
	WHERE
		hash=@hash

	SELECT
		affidavit_file_details.affidavit_file_id,
		media_months.media_month,
		systems.code,
		affidavit_file_details.checkin_invoice_count, 
		affidavit_file_details.checkin_affadavit_count, 
		affidavit_file_details.loaded_invoice_count, 
		affidavit_file_details.loaded_affidavit_count, 
		affidavit_file_details.duplicate_invoice_count, 
		affidavit_file_details.duplicate_affidavit_count	
	FROM 
		affidavit_file_details	(NOLOCK)
		JOIN systems			(NOLOCK) ON systems.id=affidavit_file_details.system_id
		JOIN media_months		(NOLOCK) ON media_months.id=affidavit_file_details.media_month_id
	WHERE
		affidavit_file_details.affidavit_file_id IN (
			SELECT id FROM affidavit_files (NOLOCK) WHERE hash=@hash
		)
END
