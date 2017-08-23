
/* END MME-102: Systems Composer - History View - Opening Zone Error*/

/* BEGIN Daypart Performance */

CREATE PROCEDURE [dbo].[usp_ARS_GetNielsenNetworkBusinessObject]
	@nielsen_network_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT
		id,
		network_rating_category_id,
		nielsen_id,
		code,
		name,
		active,
		effective_date
	FROM
		nielsen_networks WITH(NOLOCK)
	WHERE
		id=@nielsen_network_id

	SELECT
		id,
		nielsen_network_id,
		seconds_after_hour,
		length,
		effective_date,
		active
	FROM
		network_breaks WITH(NOLOCK)
	WHERE
		nielsen_network_id = @nielsen_network_id
	AND active = 1

	SELECT
		nielsen_network_rating_dayparts.nielsen_network_id,
		nielsen_network_rating_dayparts.daypart_id,
		nielsen_network_rating_dayparts.effective_date
	FROM
		nielsen_network_rating_dayparts WITH(NOLOCK)
	WHERE
		nielsen_network_rating_dayparts.nielsen_network_id=@nielsen_network_id

	SELECT
		network_traffic_dayparts.nielsen_network_id,
		network_traffic_dayparts.daypart_id,
		network_traffic_dayparts.effective_date
	FROM
		network_traffic_dayparts WITH(NOLOCK)	
	WHERE
		network_traffic_dayparts.nielsen_network_id=@nielsen_network_id
END
