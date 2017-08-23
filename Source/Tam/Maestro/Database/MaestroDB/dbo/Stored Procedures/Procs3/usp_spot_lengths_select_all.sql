CREATE PROCEDURE usp_spot_lengths_select_all
AS
SELECT
	*
FROM
	spot_lengths WITH(NOLOCK)
