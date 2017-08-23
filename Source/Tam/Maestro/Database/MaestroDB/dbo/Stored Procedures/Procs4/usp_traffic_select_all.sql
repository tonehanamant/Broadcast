
-- =============================================
-- Author:		CRUD Creator
-- Create date: 10/08/2015 01:46:42 PM
-- Description:	Auto-generated method to select all traffic records.
-- =============================================
CREATE PROCEDURE usp_traffic_select_all
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[traffic].*
	FROM
		[dbo].[traffic] WITH(NOLOCK)
END
