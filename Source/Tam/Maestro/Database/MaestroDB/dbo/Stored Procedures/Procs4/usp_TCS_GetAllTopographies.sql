-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 8/29/2013
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_TCS_GetAllTopographies]
AS
BEGIN
	SELECT
		t.*
	FROM
		topographies t (NOLOCK), 
		topography_maps tm (NOLOCK)
	WHERE 
		tm.map_set = 'traffic'
		AND t.id = tm.topography_id
	ORDER BY
		CAST(tm.map_value AS INT),
		t.name
END