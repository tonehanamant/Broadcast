-- =============================================
-- Author:		CRUD Creator
-- Create date: 11/07/2014 04:11:44 PM
-- Description:	Auto-generated method to select all proposal_invoice_four_a_log records.
-- =============================================
CREATE PROCEDURE usp_proposal_invoice_four_a_log_select_all
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[proposal_invoice_four_a_log].*
	FROM
		[dbo].[proposal_invoice_four_a_log] WITH(NOLOCK)
END

