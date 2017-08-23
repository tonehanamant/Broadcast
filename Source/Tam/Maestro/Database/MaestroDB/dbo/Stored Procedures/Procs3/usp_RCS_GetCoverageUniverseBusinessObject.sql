-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 7/24/2009
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_RCS_GetCoverageUniverseBusinessObject]
	@coverage_universe_id INT
AS
BEGIN
	SELECT
		coverage_universe_id,
		network_id,
		topography_id,
		universe,
		n.code
	FROM
		coverage_universe_details cud (NOLOCK)
		JOIN networks n (NOLOCK) ON n.id=cud.network_id
	WHERE
		coverage_universe_id=@coverage_universe_id
	ORDER BY
		n.code
END
