-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectNetworkItemsForZoneByDate]
	@active bit,
	@zone_id int,
	@effective_date datetime
AS
BEGIN
	SET NOCOUNT ON;

	SELECT 
		n.network_id,
		n.code,
		n.start_date 
	FROM 
		uvw_network_universe n (NOLOCK) 
	WHERE 
		n.network_id IN (
			SELECT 
				zn.network_id 
			FROM 
				uvw_zonenetwork_universe zn (NOLOCK) 
			WHERE 
				zn.zone_id=@zone_id
				AND ((@effective_date IS NULL AND zn.end_date IS NULL) OR (zn.start_date<=@effective_date AND (zn.end_date>=@effective_date OR end_date IS NULL)))
		) 
		AND ((@effective_date IS NULL AND n.end_date IS NULL) OR (n.start_date<=@effective_date AND (n.end_date>=@effective_date OR n.end_date IS NULL)))
		AND (@active IS NULL OR n.active=@active) 
	ORDER BY 
		n.code
END
