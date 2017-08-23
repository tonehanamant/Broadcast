
-- =============================================
-- Author:		John Carsley
-- Create date: 07/05/2011
-- Description:	Gets a receivable invoice record from the proposal_id and media month
-- =============================================
CREATE PROCEDURE [dbo].[usp_ACCT_GetReceivableInvoice]
	 @entity_id int
	,@media_month_id int 
AS
BEGIN
	SET NOCOUNT ON;

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
  FROM [receivable_invoices] WITH (NOLOCK)
  WHERE entity_id = @entity_id
  AND media_month_id = @media_month_id
  AND active = 1


END
