-- =============================================
-- Author:		CRUD Creator
-- Create date: 05/01/2014 11:23:33 AM
-- Description:	Auto-generated method to select all universes records.
-- =============================================
CREATE PROCEDURE [dbo].[usp_universes_select_all]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[universes].*
	FROM
		[dbo].[universes] WITH(NOLOCK)
END
