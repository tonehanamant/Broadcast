-- =============================================
-- Author:		CRUD Creator
-- Create date: 10/31/2014 09:41:33 AM
-- Description:	Auto-generated method to select all proposal_header_log records.
-- =============================================
CREATE PROCEDURE usp_proposal_header_log_select_all
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[proposal_header_log].*
	FROM
		[dbo].[proposal_header_log] WITH(NOLOCK)
END

