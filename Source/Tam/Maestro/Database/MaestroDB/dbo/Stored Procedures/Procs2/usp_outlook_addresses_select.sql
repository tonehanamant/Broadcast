CREATE PROCEDURE usp_outlook_addresses_select
(
	@id Int
)
AS
SELECT
	*
FROM
	outlook_addresses WITH(NOLOCK)
WHERE
	id = @id
