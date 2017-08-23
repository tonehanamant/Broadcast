-- =============================================
-- Author:		CRUD Creator
-- Create date: 11/04/2014 01:01:55 PM
-- Description:	Auto-generated method to select all proposal_tecc_log records.
-- =============================================
CREATE PROCEDURE usp_proposal_tecc_log_select_all
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[proposal_tecc_log].*
	FROM
		[dbo].[proposal_tecc_log] WITH(NOLOCK)
END

