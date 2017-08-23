-- =============================================
-- Author:		Nicholas Kheynis
-- Create date: 08/13/2014
-- Description:	Sets finance notes on updates in view invoices
-- Usage: exec usp_ACCT_UpdateFinanceNotesForProposal
-- =============================================
CREATE PROCEDURE [dbo].[usp_ACCT_UpdateFinanceNotesForProposal]
	 @invoice_number VARCHAR(63),
	 @finance_notes VARCHAR(2047)
AS
BEGIN
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
	SET NOCOUNT ON;

DECLARE @entity_id INT
SET @entity_id =(
	SELECT 
		ri.entity_id 
	FROM 
		dbo.receivable_invoices  ri
	WHERE 
		ri.invoice_number = @invoice_number
		)


UPDATE
	dbo.proposals
SET
	finance_notes = @finance_notes	
WHERE
	id = @entity_id

END


