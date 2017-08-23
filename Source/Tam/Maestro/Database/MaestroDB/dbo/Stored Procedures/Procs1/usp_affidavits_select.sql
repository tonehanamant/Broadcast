CREATE PROCEDURE usp_affidavits_select
(
	@id BigInt
)
AS
SELECT
	*
FROM
	affidavits WITH(NOLOCK)
WHERE
	id = @id
