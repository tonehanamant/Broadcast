-- =============================================
-- Author:		CRUD Creator
-- Create date: 09/21/2015 10:29:51 AM
-- Description:	Auto-generated method to select a single tam_post_proposals record.
-- =============================================
CREATE PROCEDURE [dbo].[usp_tam_post_proposals_select]
	@id Int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[tam_post_proposals].*
	FROM
		[dbo].[tam_post_proposals] WITH(NOLOCK)
	WHERE
		[id]=@id
END
