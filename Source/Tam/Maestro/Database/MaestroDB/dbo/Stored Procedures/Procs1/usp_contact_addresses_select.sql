CREATE PROCEDURE usp_contact_addresses_select
(
	@contact_id		Int,
	@address_id		Int
)
AS
SELECT
	*
FROM
	contact_addresses WITH(NOLOCK)
WHERE
	contact_id=@contact_id
	AND
	address_id=@address_id

