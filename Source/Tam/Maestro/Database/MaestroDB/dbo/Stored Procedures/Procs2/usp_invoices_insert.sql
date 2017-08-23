-- =============================================
-- Author:		CRUD Creator
-- Create date: 03/18/2014 10:22:04 AM
-- Description:	Auto-generated method to insert a invoices record.
-- =============================================
CREATE PROCEDURE [dbo].[usp_invoices_insert]
	@id INT OUTPUT,
	@system_id INT,
	@invoicing_system_id INT,
	@affidavit_file_id INT,
	@media_month_id INT,
	@external_id VARCHAR(15),
	@external_id_suffix VARCHAR(15),
	@invoice_date DATETIME,
	@invoice_gross_due INT,
	@invoice_spot_num INT,
	@invoice_estimate_code VARCHAR(63),
	@invoice_con_start_date DATETIME,
	@invoice_con_end_date DATETIME,
	@invoice_external_id VARCHAR(15),
	@invoice_system_name VARCHAR(63),
	@invoice_media_month VARCHAR(15),
	@date_created DATETIME,
	@invoice_net_due INT,
	@invoice_agency_commission INT,
	@address_line_1 VARCHAR(63),
	@address_line_2 VARCHAR(63),
	@address_line_3 VARCHAR(63),
	@address_line_4 VARCHAR(63),
	@address_line_5 VARCHAR(63),
	@computer_system VARCHAR(15),
	@business_id INT
AS
BEGIN
	INSERT INTO [dbo].[invoices]
	(
		[system_id],
		[invoicing_system_id],
		[affidavit_file_id],
		[media_month_id],
		[external_id],
		[external_id_suffix],
		[invoice_date],
		[invoice_gross_due],
		[invoice_spot_num],
		[invoice_estimate_code],
		[invoice_con_start_date],
		[invoice_con_end_date],
		[invoice_external_id],
		[invoice_system_name],
		[invoice_media_month],
		[date_created],
		[invoice_net_due],
		[invoice_agency_commission],
		[address_line_1],
		[address_line_2],
		[address_line_3],
		[address_line_4],
		[address_line_5],
		[computer_system],
		[business_id]
	)
	VALUES
	(
		@system_id,
		@invoicing_system_id,
		@affidavit_file_id,
		@media_month_id,
		@external_id,
		@external_id_suffix,
		@invoice_date,
		@invoice_gross_due,
		@invoice_spot_num,
		@invoice_estimate_code,
		@invoice_con_start_date,
		@invoice_con_end_date,
		@invoice_external_id,
		@invoice_system_name,
		@invoice_media_month,
		@date_created,
		@invoice_net_due,
		@invoice_agency_commission,
		@address_line_1,
		@address_line_2,
		@address_line_3,
		@address_line_4,
		@address_line_5,
		@computer_system,
		@business_id
	)

	SELECT
		@id = SCOPE_IDENTITY()
END
