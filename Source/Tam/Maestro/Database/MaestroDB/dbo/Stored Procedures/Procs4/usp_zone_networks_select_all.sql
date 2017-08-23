
-- =============================================
-- Author:		CRUD Creator
-- Create date: 11/29/2016 02:29:17 PM
-- Description:	Auto-generated method to select all zone_networks records.
-- =============================================
CREATE PROCEDURE usp_zone_networks_select_all
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[zone_networks].*
	FROM
		[dbo].[zone_networks] WITH(NOLOCK)
END
