-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_RCS_GetCoverageUniverseDetails]
	@coverage_universe_id INT
AS
BEGIN
	SELECT
		coverage_universe_id,
		network_id,
		topography_id,
		universe
	FROM
		coverage_universe_details (NOLOCK)
	WHERE
		coverage_universe_id=@coverage_universe_id
END
