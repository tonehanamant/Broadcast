CREATE PROCEDURE usp_affidavit_file_details_update
(
	@id		Int,
	@system_id		Int,
	@affidavit_file_id		Int,
	@media_month_id		Int,
	@checkin_invoice_count		Int,
	@checkin_affadavit_count		Int,
	@loaded_invoice_count		Int,
	@loaded_affidavit_count		Int,
	@duplicate_invoice_count		Int,
	@duplicate_affidavit_count		Int
)
AS
UPDATE affidavit_file_details SET
	system_id = @system_id,
	affidavit_file_id = @affidavit_file_id,
	media_month_id = @media_month_id,
	checkin_invoice_count = @checkin_invoice_count,
	checkin_affadavit_count = @checkin_affadavit_count,
	loaded_invoice_count = @loaded_invoice_count,
	loaded_affidavit_count = @loaded_affidavit_count,
	duplicate_invoice_count = @duplicate_invoice_count,
	duplicate_affidavit_count = @duplicate_affidavit_count
WHERE
	id = @id

