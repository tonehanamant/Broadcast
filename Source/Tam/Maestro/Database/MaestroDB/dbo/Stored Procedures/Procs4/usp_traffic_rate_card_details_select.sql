CREATE PROCEDURE usp_traffic_rate_card_details_select
(
	@id Int
)
AS
SELECT
	*
FROM
	traffic_rate_card_details WITH(NOLOCK)
WHERE
	id = @id
