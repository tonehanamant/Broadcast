
-- =============================================
-- Author:		CRUD Creator
-- Create date: 01/30/2017 12:09:59 PM
-- Description:	Auto-generated method to select all proposal_marry_candidates records.
-- =============================================
CREATE PROCEDURE usp_proposal_marry_candidates_select_all
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[proposal_marry_candidates].*
	FROM
		[dbo].[proposal_marry_candidates] WITH(NOLOCK)
END

