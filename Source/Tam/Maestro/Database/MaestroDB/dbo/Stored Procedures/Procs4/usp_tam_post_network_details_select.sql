CREATE PROCEDURE usp_tam_post_network_details_select
(
	@id Int
)
AS
SELECT
	*
FROM
	tam_post_network_details WITH(NOLOCK)
WHERE
	id = @id
