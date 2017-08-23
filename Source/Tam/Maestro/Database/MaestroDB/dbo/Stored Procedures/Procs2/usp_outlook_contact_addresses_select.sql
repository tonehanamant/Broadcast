CREATE PROCEDURE usp_outlook_contact_addresses_select
(
	@outlook_contact_id		Int,
	@outlook_address_id		Int
)
AS
SELECT
	*
FROM
	outlook_contact_addresses WITH(NOLOCK)
WHERE
	outlook_contact_id=@outlook_contact_id
	AND
	outlook_address_id=@outlook_address_id

