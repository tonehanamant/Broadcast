-- =============================================
-- Author:		CRUD Creator
-- Create date: 09/21/2015 10:29:51 AM
-- Description:	Auto-generated method to select all tam_post_proposals records.
-- =============================================
CREATE PROCEDURE [dbo].[usp_tam_post_proposals_select_all]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[tam_post_proposals].*
	FROM
		[dbo].[tam_post_proposals] WITH(NOLOCK)
END
