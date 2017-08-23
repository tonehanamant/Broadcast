CREATE PROCEDURE usp_cmw_invoice_details_insert
(
	@cmw_invoice_id		Int,
	@cmw_traffic_id		Int
)
AS
INSERT INTO cmw_invoice_details
(
	cmw_invoice_id,
	cmw_traffic_id
)
VALUES
(
	@cmw_invoice_id,
	@cmw_traffic_id
)

