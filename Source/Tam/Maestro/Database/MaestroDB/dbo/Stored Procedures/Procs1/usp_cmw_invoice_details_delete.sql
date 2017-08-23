CREATE PROCEDURE usp_cmw_invoice_details_delete
(
	@cmw_invoice_id		Int,
	@cmw_traffic_id		Int)
AS
DELETE FROM
	cmw_invoice_details
WHERE
	cmw_invoice_id = @cmw_invoice_id
 AND
	cmw_traffic_id = @cmw_traffic_id
