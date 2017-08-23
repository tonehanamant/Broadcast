-- =============================================
-- Author:		CRUD Creator
-- Create date: 03/25/2016 01:41:05 PM
-- Description:	Auto-generated method to select a single proposals record.
-- =============================================
CREATE PROCEDURE usp_proposals_select
	@id Int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[proposals].*
	FROM
		[dbo].[proposals] WITH(NOLOCK)
	WHERE
		[id]=@id
END
