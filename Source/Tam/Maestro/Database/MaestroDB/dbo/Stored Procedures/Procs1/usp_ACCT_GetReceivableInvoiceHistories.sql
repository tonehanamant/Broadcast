
-- =============================================
-- Author:		John Carsley
-- Create date: 08/24/2011
-- Description:	Gets receivable invoice history records from the proposal_id and media month
-- =============================================
CREATE PROCEDURE [dbo].[usp_ACCT_GetReceivableInvoiceHistories]
	 @entity_id int
	,@media_month_id int 
AS
BEGIN
	SET NOCOUNT ON;

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
      ,[active]
      ,[ISCI_codes]
      ,[invoice_type_id]
      ,[effective_date]
      ,[date_created]
      ,[date_modified]
      ,[modified_by]
      ,[end_date]
  FROM [maestro_john].[dbo].[receivable_invoice_histories] WITH (NOLOCK)
  WHERE entity_id = @entity_id
  AND media_month_id = @media_month_id
END
