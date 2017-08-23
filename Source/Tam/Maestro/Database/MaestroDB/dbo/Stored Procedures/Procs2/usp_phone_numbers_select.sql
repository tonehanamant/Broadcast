CREATE PROCEDURE usp_phone_numbers_select
(
	@id Int
)
AS
SELECT
	*
FROM
	phone_numbers WITH(NOLOCK)
WHERE
	id = @id
