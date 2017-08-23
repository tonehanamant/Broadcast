
CREATE PROCEDURE [dbo].[usp_receivable_invoice_histories_delete]
(
	@start_date		DateTime,
	@media_month_id		Int,
	@entity_id		Int,
	@active		Bit)
AS
DELETE FROM
	receivable_invoice_histories
WHERE
	start_date = @start_date
 AND
	media_month_id = @media_month_id
 AND
	entity_id = @entity_id
 AND
	active = @active

