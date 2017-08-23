CREATE PROCEDURE usp_phone_number_types_select
(
	@id Int
)
AS
SELECT
	*
FROM
	phone_number_types WITH(NOLOCK)
WHERE
	id = @id
