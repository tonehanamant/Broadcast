
-- =============================================
-- Author:		CRUD Creator
-- Create date: 07/01/2015 12:40:31 PM
-- Description:	Auto-generated method to select a single tam_posts record.
-- =============================================
CREATE PROCEDURE usp_tam_posts_select
	@id Int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[tam_posts].*
	FROM
		[dbo].[tam_posts] WITH(NOLOCK)
	WHERE
		[id]=@id
END
