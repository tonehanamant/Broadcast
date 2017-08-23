CREATE PROCEDURE [dbo].[usp_nielsen_county_universe_estimates_select_all]
AS
SELECT
	*
FROM
	nielsen_county_universe_estimates WITH(NOLOCK)
