CREATE PROCEDURE usp_outlook_phone_numbers_select
(
	@id Int
)
AS
SELECT
	*
FROM
	outlook_phone_numbers WITH(NOLOCK)
WHERE
	id = @id
