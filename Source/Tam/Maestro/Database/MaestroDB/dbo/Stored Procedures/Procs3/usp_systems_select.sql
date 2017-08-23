
-- =============================================
-- Author:		CRUD Creator
-- Create date: 05/11/2017 11:10:53 AM
-- Description:	Auto-generated method to select a single systems record.
-- =============================================
CREATE PROCEDURE usp_systems_select
	@id Int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[systems].*
	FROM
		[dbo].[systems] WITH(NOLOCK)
	WHERE
		[id]=@id
END
