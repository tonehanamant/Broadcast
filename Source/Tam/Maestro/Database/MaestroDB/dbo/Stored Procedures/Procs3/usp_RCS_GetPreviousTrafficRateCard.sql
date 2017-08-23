CREATE PROCEDURE [dbo].[usp_RCS_GetPreviousTrafficRateCard]
@traffic_rate_card_id int
AS
declare @start_date datetime
declare @topography_id int

SELECT @start_date = start_date, @topography_id = topography_id from traffic_rate_cards (NOLOCK) where id = @traffic_rate_card_id

BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT
		*
	FROM
		traffic_rate_cards trc (NOLOCK)
	WHERE
		topography_id = @topography_id
	AND
		id <> @traffic_rate_card_id
	AND
		convert(varchar, end_date, 110) = convert(varchar, DATEADD(DAY,-1,@start_date), 110)
END
