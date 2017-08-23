
CREATE PROCEDURE [dbo].[usp_receivable_invoice_histories_select]
(
	@start_date		DateTime,
	@media_month_id		Int,
	@entity_id		Int,
	@active		Bit
)
AS
SELECT [receivable_invoice_id]
      ,[start_date]
      ,[media_month_id]
      ,[entity_id]
      ,[customer_number]
      ,[invoice_number]
      ,[special_notes]
      ,[total_units]
      ,[total_due_gross]
      ,[total_due_net]
      ,[total_credits]
      ,[document_id]
      ,[is_mailed]
      ,[ISCI_codes]
      ,[invoice_type_id]
      ,[active]
      ,[effective_date]
      ,[date_created]
      ,[date_modified]
      ,[modified_by]
      ,[end_date]
  FROM
	receivable_invoice_histories WITH(NOLOCK)
WHERE
	start_date=@start_date
	AND
	media_month_id=@media_month_id
	AND
	entity_id=@entity_id
	AND
	active=@active


