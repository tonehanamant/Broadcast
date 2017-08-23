CREATE PROCEDURE usp_company_addresses_select
(
	@company_id		Int,
	@address_id		Int
)
AS
SELECT
	*
FROM
	company_addresses WITH(NOLOCK)
WHERE
	company_id=@company_id
	AND
	address_id=@address_id

