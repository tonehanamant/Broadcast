CREATE PROCEDURE usp_cmw_invoice_details_select
(
	@cmw_invoice_id		Int,
	@cmw_traffic_id		Int
)
AS
SELECT
	*
FROM
	cmw_invoice_details WITH(NOLOCK)
WHERE
	cmw_invoice_id=@cmw_invoice_id
	AND
	cmw_traffic_id=@cmw_traffic_id

