CREATE PROCEDURE usp_contact_addresses_insert
(
	@contact_id		Int,
	@address_id		Int
)
AS
INSERT INTO contact_addresses
(
	contact_id,
	address_id
)
VALUES
(
	@contact_id,
	@address_id
)

