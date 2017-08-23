-- =============================================
-- Author:Stephen DeFusco
-- Create date: 5/7/2013
-- Description:
-- =============================================
CREATE PROCEDURE usp_PLS_SelectNetworkMapHelperData
	@map_set VARCHAR(31),
	@effective_date DATETIME
AS
BEGIN
	SET NOCOUNT ON;
	
	SELECT
		nm.network_map_id, 
		nm.start_date, 
		nm.network_id, 
		nm.map_set, 
		nm.map_value, 
		nm.active, 
		nm.flag, 
		CASE WHEN nm.end_date IS NULL THEN '9999-12-31 23:59:59.997' ELSE nm.end_date END,
		
		nn.nielsen_network_id, 
		nn.start_date, 
		nn.network_rating_category_id, 
		nn.nielsen_id, 
		nn.code, 
		nn.name, 
		nn.active, 
		CASE WHEN nn.end_date IS NULL THEN '9999-12-31 23:59:59.997' ELSE nn.end_date END
	FROM
		dbo.uvw_networkmap_universe nm (NOLOCK)
		JOIN dbo.uvw_networkmap_universe nm_nielsen (NOLOCK) ON nm_nielsen.network_id=nm.network_id
			AND nm_nielsen.map_set='Nielsen'
			AND nm_nielsen.start_date<=@effective_date AND (nm_nielsen.end_date>=@effective_date OR nm_nielsen.end_date IS NULL)
		JOIN dbo.uvw_nielsen_network_universes nn (NOLOCK) ON nn.nielsen_id=CAST(nm_nielsen.map_value AS INT)
			AND nn.start_date<=@effective_date AND (nn.end_date>=@effective_date OR nn.end_date IS NULL)
	WHERE
		nm.map_set=@map_set
		AND nm.start_date<=@effective_date AND (nm.end_date>=@effective_date OR nm.end_date IS NULL)
END
