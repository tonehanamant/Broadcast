CREATE PROCEDURE usp_affidavit_file_details_insert
(
	@id		Int		OUTPUT,
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
INSERT INTO affidavit_file_details
(
	system_id,
	affidavit_file_id,
	media_month_id,
	checkin_invoice_count,
	checkin_affadavit_count,
	loaded_invoice_count,
	loaded_affidavit_count,
	duplicate_invoice_count,
	duplicate_affidavit_count
)
VALUES
(
	@system_id,
	@affidavit_file_id,
	@media_month_id,
	@checkin_invoice_count,
	@checkin_affadavit_count,
	@loaded_invoice_count,
	@loaded_affidavit_count,
	@duplicate_invoice_count,
	@duplicate_affidavit_count
)

SELECT
	@id = SCOPE_IDENTITY()

