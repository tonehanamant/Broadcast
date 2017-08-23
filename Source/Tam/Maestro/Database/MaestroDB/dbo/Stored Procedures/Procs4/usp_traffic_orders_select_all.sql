
-- =============================================
-- Author:		CRUD Creator
-- Create date: 08/25/2016 04:41:41 PM
-- Description:	Auto-generated method to select all traffic_orders records.
-- =============================================
CREATE PROCEDURE usp_traffic_orders_select_all
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[traffic_orders].*
	FROM
		[dbo].[traffic_orders] WITH(NOLOCK)
END

