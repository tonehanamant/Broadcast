
-- =============================================
-- Author:		CRUD Creator
-- Create date: 07/01/2015 12:40:30 PM
-- Description:	Auto-generated method to select all tam_posts records.
-- =============================================
CREATE PROCEDURE usp_tam_posts_select_all
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[tam_posts].*
	FROM
		[dbo].[tam_posts] WITH(NOLOCK)
END
