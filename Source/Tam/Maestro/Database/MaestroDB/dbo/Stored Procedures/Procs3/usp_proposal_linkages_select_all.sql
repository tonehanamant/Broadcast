-- =============================================
-- Author:		CRUD Creator
-- Create date: 08/25/2014 08:11:16 AM
-- Description:	Auto-generated method to select all proposal_linkages records.
-- =============================================
CREATE PROCEDURE [dbo].[usp_proposal_linkages_select_all]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[proposal_linkages].*
	FROM
		[dbo].[proposal_linkages] WITH(NOLOCK)
END
