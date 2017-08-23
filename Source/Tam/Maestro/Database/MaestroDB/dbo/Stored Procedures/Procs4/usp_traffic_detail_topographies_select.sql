
-- =============================================
-- Author:		CRUD Creator
-- Create date: 06/18/2015 02:09:00 PM
-- Description:	Auto-generated method to delete or potentionally disable a traffic_detail_topographies record.
-- =============================================
CREATE PROCEDURE usp_traffic_detail_topographies_select
	@traffic_detail_week_id INT,
	@topography_id INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[traffic_detail_topographies].*
	FROM
		[dbo].[traffic_detail_topographies] WITH(NOLOCK)
	WHERE
		[traffic_detail_week_id]=@traffic_detail_week_id
		AND [topography_id]=@topography_id
END
