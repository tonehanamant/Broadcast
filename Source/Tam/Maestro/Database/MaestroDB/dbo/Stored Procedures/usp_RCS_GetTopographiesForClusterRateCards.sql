
CREATE PROCEDURE usp_RCS_GetTopographiesForClusterRateCards
AS
BEGIN
	SET NOCOUNT ON;

    SELECT
		t.*
	FROM
		dbo.topographies t (NOLOCK)
	WHERE
		t.id IN (
			SELECT DISTINCT topography_id FROM dbo.cluster_rate_cards trc (NOLOCK)
		)
	ORDER BY
		t.name
END