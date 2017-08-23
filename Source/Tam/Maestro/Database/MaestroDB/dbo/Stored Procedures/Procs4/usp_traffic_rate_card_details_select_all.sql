CREATE PROCEDURE usp_traffic_rate_card_details_select_all
AS
SELECT
	*
FROM
	traffic_rate_card_details WITH(NOLOCK)
