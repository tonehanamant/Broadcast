-- =============================================
-- Author:		CRUD Creator
-- Create date: 08/11/2014 10:31:07 AM
-- Description:	Auto-generated method to select a single companies record.
-- =============================================
CREATE PROCEDURE usp_companies_select
	@id Int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[companies].*
	FROM
		[dbo].[companies] WITH(NOLOCK)
	WHERE
		[id]=@id
END

