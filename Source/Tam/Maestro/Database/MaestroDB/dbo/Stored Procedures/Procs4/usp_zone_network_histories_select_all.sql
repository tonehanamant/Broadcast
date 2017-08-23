
-- =============================================
-- Author:		CRUD Creator
-- Create date: 11/29/2016 02:29:16 PM
-- Description:	Auto-generated method to select all zone_network_histories records.
-- =============================================
CREATE PROCEDURE usp_zone_network_histories_select_all
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[zone_network_histories].*
	FROM
		[dbo].[zone_network_histories] WITH(NOLOCK)
END
