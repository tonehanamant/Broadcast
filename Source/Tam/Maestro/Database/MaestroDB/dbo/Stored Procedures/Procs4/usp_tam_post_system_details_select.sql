CREATE PROCEDURE usp_tam_post_system_details_select
(
	@id Int
)
AS
SELECT
	*
FROM
	tam_post_system_details WITH(NOLOCK)
WHERE
	id = @id
