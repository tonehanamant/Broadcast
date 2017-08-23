CREATE PROCEDURE [dbo].[usp_invoices_update]
(
	@id		Int,
	@system_id		Int,
	@invoicing_system_id		Int,
	@affidavit_file_id		Int,
	@media_month_id		Int,
	@external_id		VarChar(15),
	@external_id_suffix		VarChar(15),
	@invoice_date		DateTime,
	@invoice_gross_due		Int,
	@invoice_spot_num		Int,
	@invoice_estimate_code		VarChar(63),
	@invoice_con_start_date		DateTime,
	@invoice_con_end_date		DateTime,
	@invoice_external_id		VarChar(15),
	@invoice_system_name		VarChar(63),
	@invoice_media_month		VarChar(15),
	@date_created		DateTime,
	@invoice_net_due		Int,
	@invoice_agency_commission		Int,
	@address_line_1		VarChar(63),
	@address_line_2		VarChar(63),
	@address_line_3		VarChar(63),
	@address_line_4		VarChar(63),
	@address_line_5		VarChar(63),
	@computer_system		VarChar(15),
	@business_id INT
)
AS
BEGIN
UPDATE dbo.invoices SET
	system_id = @system_id,
	invoicing_system_id = @invoicing_system_id,
	affidavit_file_id = @affidavit_file_id,
	media_month_id = @media_month_id,
	external_id = @external_id,
	external_id_suffix = @external_id_suffix,
	invoice_date = @invoice_date,
	invoice_gross_due = @invoice_gross_due,
	invoice_spot_num = @invoice_spot_num,
	invoice_estimate_code = @invoice_estimate_code,
	invoice_con_start_date = @invoice_con_start_date,
	invoice_con_end_date = @invoice_con_end_date,
	invoice_external_id = @invoice_external_id,
	invoice_system_name = @invoice_system_name,
	invoice_media_month = @invoice_media_month,
	date_created = @date_created,
	invoice_net_due = @invoice_net_due,
	invoice_agency_commission = @invoice_agency_commission,
	address_line_1 = @address_line_1,
	address_line_2 = @address_line_2,
	address_line_3 = @address_line_3,
	address_line_4 = @address_line_4,
	address_line_5 = @address_line_5,
	computer_system = @computer_system,
	business_id = @business_id
WHERE
	id = @id

END
