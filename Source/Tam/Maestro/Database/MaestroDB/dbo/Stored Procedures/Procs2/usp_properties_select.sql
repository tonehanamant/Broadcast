CREATE PROCEDURE usp_properties_select
(
	@id Int
)
AS
SELECT
	*
FROM
	properties WITH(NOLOCK)
WHERE
	id = @id
