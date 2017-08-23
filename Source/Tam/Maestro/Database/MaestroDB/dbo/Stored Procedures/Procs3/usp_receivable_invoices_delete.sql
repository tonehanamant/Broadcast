
CREATE PROCEDURE [dbo].[usp_receivable_invoices_delete]
(
	@id Int
,
	@effective_date DateTime
)
AS
UPDATE receivable_invoices SET active=0, effective_date=@effective_date WHERE id=@id

