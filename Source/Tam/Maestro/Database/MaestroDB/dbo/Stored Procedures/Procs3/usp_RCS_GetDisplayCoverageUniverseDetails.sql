-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 8/21/2013
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_RCS_GetDisplayCoverageUniverseDetails
	@coverage_universe_id INT
AS
BEGIN
	SET NOCOUNT ON;

    SELECT 
		t.name 'topography',
		n.code 'network',
		CAST(cud.universe AS INT) 'universe'
	FROM 
		dbo.coverage_universe_details cud (NOLOCK) 
		JOIN dbo.topographies t (NOLOCK) ON t.id=cud.topography_id
		JOIN dbo.networks n (NOLOCK) ON n.id=cud.network_id
	WHERE 
		cud.coverage_universe_id=@coverage_universe_id
	ORDER BY
		t.name,
		n.code
END
