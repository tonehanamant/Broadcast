
/* END Proposal Advanced TV */

/* BEGIN Affidavit Composer Delete Affidavits */

-- usp_ACS_MoveInvoiceAndSpotsToDeletedTables 11842660
CREATE PROCEDURE [dbo].[usp_ACS_MoveInvoiceAndSpotsToDeletedTables]
	@invoice_id INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @rowsAffected INT
	DECLARE @textTimestamp VARCHAR(63)
	DECLARE @media_month_id INT
	SET @rowsAffected = 0;
	SET @media_month_id = (SELECT media_month_id FROM invoices (NOLOCK) WHERE id=@invoice_id);

	SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121)
	RAISERROR('%s - Starting [invoice_id=%d]...', 0, 1, @textTimestamp, @invoice_id) WITH NOWAIT;

	BEGIN TRANSACTION

	-- MOVE invoice and affidavit(s) to "deleted" tables
	SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121)
	RAISERROR('%s - Moving invoices to "deleted_invoices"...', 0, 1, @textTimestamp) WITH NOWAIT;
	INSERT INTO deleted_invoices (id, system_id, invoicing_system_id, affidavit_file_id, media_month_id, external_id, external_id_suffix, invoice_date, invoice_gross_due, invoice_spot_num, invoice_estimate_code, invoice_con_start_date, invoice_con_end_date, invoice_external_id, invoice_system_name, invoice_media_month, date_created, invoice_net_due, invoice_agency_commission, address_line_1, address_line_2, address_line_3, address_line_4, address_line_5, computer_system, business_id)
		SELECT 
			id, system_id, invoicing_system_id, affidavit_file_id, media_month_id, external_id, external_id_suffix, invoice_date, invoice_gross_due, invoice_spot_num, invoice_estimate_code, invoice_con_start_date, invoice_con_end_date, invoice_external_id, invoice_system_name, invoice_media_month, date_created, invoice_net_due, invoice_agency_commission, address_line_1, address_line_2, address_line_3, address_line_4, address_line_5, computer_system, business_id
		FROM
			invoices (NOLOCK)
		WHERE 
			id=@invoice_id
	SET @rowsAffected = @rowsAffected + @@ROWCOUNT;

	SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121)
	RAISERROR('%s - Moving affidavits to "deleted_affidavits"...', 0, 1, @textTimestamp) WITH NOWAIT;
	INSERT INTO deleted_affidavits (id, media_month_id, status_id, invoice_id, traffic_id, material_id, zone_id, network_id, spot_length_id, air_date, air_time, rate, affidavit_file_line, affidavit_air_date, affidavit_air_time, affidavit_length, affidavit_copy, affidavit_net, affidavit_syscode, affidavit_rate, hash, subscribers, [program_name], adjusted_air_date, adjusted_air_time)
		SELECT 
			id, media_month_id, status_id, invoice_id, traffic_id, material_id, zone_id, network_id, spot_length_id, air_date, air_time, rate, affidavit_file_line, affidavit_air_date, affidavit_air_time, affidavit_length, affidavit_copy, affidavit_net, affidavit_syscode, affidavit_rate, hash, subscribers, [program_name], adjusted_air_date, adjusted_air_time
		FROM
			affidavits (NOLOCK)
		WHERE 
			media_month_id=@media_month_id
			AND invoice_id=@invoice_id
	SET @rowsAffected = @rowsAffected + @@ROWCOUNT;

	-- DELETE affidavits
	SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121)
	RAISERROR('%s - Deleting "affidavits"...', 0, 1, @textTimestamp) WITH NOWAIT;
	DELETE FROM affidavits WHERE media_month_id=@media_month_id 
		AND id IN (SELECT a.id FROM affidavits a (NOLOCK) WHERE a.media_month_id=@media_month_id AND a.invoice_id=@invoice_id)
	SET @rowsAffected = @rowsAffected + @@ROWCOUNT;

	-- DELETE invoices
	SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121)
	RAISERROR('%s - Deleting "invoices"...', 0, 1, @textTimestamp) WITH NOWAIT;

	DELETE FROM invoices WHERE id=@invoice_id
	SET @rowsAffected = @rowsAffected + @@ROWCOUNT;

	COMMIT TRANSACTION

	SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121)
	RAISERROR('%s - Finsihed [invoice_id=%d]...', 0, 1, @textTimestamp, @invoice_id) WITH NOWAIT;

	RETURN 	@rowsAffected;
END
