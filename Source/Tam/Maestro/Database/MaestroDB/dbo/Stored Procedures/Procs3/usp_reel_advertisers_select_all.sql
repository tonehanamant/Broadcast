CREATE PROCEDURE usp_reel_advertisers_select_all
AS
SELECT
	*
FROM
	reel_advertisers WITH(NOLOCK)
