

CREATE PROCEDURE usp_network_rate_card_details_select
(
	@id Int
)
AS
BEGIN
SELECT
	*
FROM
	dbo.network_rate_card_details WITH(NOLOCK)
WHERE
	id = @id
END
