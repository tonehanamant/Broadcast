
CREATE PROCEDURE [dbo].[usp_receivable_invoices_select_all]
AS
SELECT [id]
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
  FROM
	receivable_invoices WITH(NOLOCK)

