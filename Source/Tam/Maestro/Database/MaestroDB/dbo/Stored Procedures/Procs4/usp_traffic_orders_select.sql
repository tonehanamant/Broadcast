
-- =============================================
-- Author:		CRUD Creator
-- Create date: 08/25/2016 04:41:41 PM
-- Description:	Auto-generated method to select a single traffic_orders record.
-- =============================================
CREATE PROCEDURE usp_traffic_orders_select
	@id Int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[traffic_orders].*
	FROM
		[dbo].[traffic_orders] WITH(NOLOCK)
	WHERE
		[id]=@id
END
