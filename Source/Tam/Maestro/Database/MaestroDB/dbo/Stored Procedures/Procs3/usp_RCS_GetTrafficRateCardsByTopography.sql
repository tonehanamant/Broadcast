CREATE PROCEDURE [dbo].[usp_RCS_GetTrafficRateCardsByTopography]
@topography_id int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT
		*
	FROM
		traffic_rate_cards trc (NOLOCK)
	JOIN
		topographies t (NOLOCK) on t.id = trc.topography_id
	WHERE
		t.id = @topography_id
	AND
		trc.end_date is NULL
END
