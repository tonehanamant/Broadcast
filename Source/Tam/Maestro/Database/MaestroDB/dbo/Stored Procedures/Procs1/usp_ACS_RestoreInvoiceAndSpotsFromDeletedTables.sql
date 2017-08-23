-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 11/19/2014
-- Description:	Restores invoices and affidavits (by invoices.id) from deleted_invoices and deleted_affidavits.
-- =============================================
/* 
	usp_ACS_RestoreInvoiceAndSpotsFromDeletedTables 20241939
	select * from deleted_invoices where id=20241939 select * from deleted_affidavits where media_month_id=396 and invoice_id=20241939
	select * from invoices where id=20241939 select * from affidavits where media_month_id=396 and invoice_id=20241939
*/
CREATE PROCEDURE [dbo].[usp_ACS_RestoreInvoiceAndSpotsFromDeletedTables]
	@invoice_id INT
AS
BEGIN
	DECLARE @rowsAffected INT
	DECLARE @textTimestamp VARCHAR(63)
	DECLARE @media_month_id INT
	SET @rowsAffected = 0;
	SET @media_month_id = (SELECT media_month_id FROM deleted_invoices (NOLOCK) WHERE id=@invoice_id);
	
	SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121)
	RAISERROR('%s - Starting...', 0, 1, @textTimestamp) WITH NOWAIT;
	
	BEGIN TRANSACTION

	-- MOVE deleted_invoices to "invoices" table
	SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121)
	RAISERROR('%s - Moving deleted_invoices to "invoices"...', 0, 1, @textTimestamp) WITH NOWAIT;
	SET IDENTITY_INSERT invoices ON
	INSERT INTO invoices (id, system_id, invoicing_system_id, affidavit_file_id, media_month_id, external_id, external_id_suffix, invoice_date, invoice_gross_due, invoice_spot_num, invoice_estimate_code, invoice_con_start_date, invoice_con_end_date, invoice_external_id, invoice_system_name, invoice_media_month, date_created, invoice_net_due, invoice_agency_commission, address_line_1, address_line_2, address_line_3, address_line_4, address_line_5, computer_system, business_id)
		SELECT 
			id, system_id, invoicing_system_id, affidavit_file_id, media_month_id, external_id, external_id_suffix, invoice_date, invoice_gross_due, invoice_spot_num, invoice_estimate_code, invoice_con_start_date, invoice_con_end_date, invoice_external_id, invoice_system_name, invoice_media_month, date_created, invoice_net_due, invoice_agency_commission, address_line_1, address_line_2, address_line_3, address_line_4, address_line_5, computer_system, business_id
		FROM
			deleted_invoices (NOLOCK)
		WHERE 
			id=@invoice_id
	SET @rowsAffected = @rowsAffected + @@ROWCOUNT;
	SET IDENTITY_INSERT invoices OFF
	
	
	-- MOVE deleted_affidavits to "affidavits" tables
	SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121)
	RAISERROR('%s - Moving deleted_affidavits to "affidavits"...', 0, 1, @textTimestamp) WITH NOWAIT;
	SET IDENTITY_INSERT affidavits ON
	INSERT INTO affidavits (id, media_month_id, status_id, invoice_id, traffic_id, material_id, zone_id, network_id, spot_length_id, air_date, air_time, rate, affidavit_file_line, affidavit_air_date, affidavit_air_time, affidavit_length, affidavit_copy, affidavit_net, affidavit_syscode, affidavit_rate, hash, subscribers, [program_name], adjusted_air_date, adjusted_air_time)
		SELECT 
			id, media_month_id, status_id, invoice_id, traffic_id, material_id, zone_id, network_id, spot_length_id, air_date, air_time, rate, affidavit_file_line, affidavit_air_date, affidavit_air_time, affidavit_length, affidavit_copy, affidavit_net, affidavit_syscode, affidavit_rate, hash, subscribers, [program_name], adjusted_air_date, adjusted_air_time
		FROM
			deleted_affidavits (NOLOCK)
		WHERE 
			media_month_id=@media_month_id
			AND invoice_id=@invoice_id
	SET @rowsAffected = @rowsAffected + @@ROWCOUNT;
	SET IDENTITY_INSERT affidavits OFF
	
	
	-- DELETE deleted_affidavits
	SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121)
	RAISERROR('%s - Deleting "deleted_affidavits"...', 0, 1, @textTimestamp) WITH NOWAIT;
	DELETE FROM deleted_affidavits WHERE media_month_id=@media_month_id 
		AND id IN (SELECT a.id FROM deleted_affidavits a (NOLOCK) WHERE a.media_month_id=@media_month_id AND a.invoice_id=@invoice_id)
	SET @rowsAffected = @rowsAffected + @@ROWCOUNT;


	-- DELETE deleted_invoices
	SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121)
	RAISERROR('%s - Deleting "deleted_invoices"...', 0, 1, @textTimestamp) WITH NOWAIT;
	DELETE FROM deleted_invoices WHERE id=@invoice_id
	SET @rowsAffected = @rowsAffected + @@ROWCOUNT;

	COMMIT TRANSACTION

	SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121)
	RAISERROR('%s - Finsihed...', 0, 1, @textTimestamp) WITH NOWAIT;

	RETURN 	@rowsAffected;
END
