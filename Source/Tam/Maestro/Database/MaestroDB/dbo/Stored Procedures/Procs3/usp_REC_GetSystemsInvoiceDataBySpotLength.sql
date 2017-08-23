CREATE PROCEDURE [dbo].[usp_REC_GetSystemsInvoiceDataBySpotLength]
	@media_month_id int,
	@spot_length_id int,
	@system_id int
AS
	select 
		systems.name, 
		invoices.system_id, 
		affidavits.spot_length_id, 
		sum(affidavits.rate) / 100.0
	from 
		affidavits (NOLOCK)
		join invoices (NOLOCK) on invoices.id = affidavits.invoice_id
		join systems (NOLOCK) on systems.id = invoices.system_id
	where 
		affidavits.media_month_id=@media_month_id
		and affidavits.zone_id is not null
		and affidavits.spot_length_id is not null
		and affidavits.status_id = 1
		and affidavits.spot_length_id = @spot_length_id
		and invoices.system_id = @system_id
	group by 
		systems.name, 
		invoices.system_id, 
		affidavits.spot_length_id
