-- =============================================
-- Author:		CRUD Creator
-- Create date: 08/11/2014 10:31:06 AM
-- Description:	Auto-generated method to select all companies records.
-- =============================================
CREATE PROCEDURE usp_companies_select_all
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[companies].*
	FROM
		[dbo].[companies] WITH(NOLOCK)
END

