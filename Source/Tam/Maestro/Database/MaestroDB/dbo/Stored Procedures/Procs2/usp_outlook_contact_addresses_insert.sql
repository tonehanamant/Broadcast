CREATE PROCEDURE usp_outlook_contact_addresses_insert
(
	@outlook_contact_id		Int,
	@outlook_address_id		Int
)
AS
INSERT INTO outlook_contact_addresses
(
	outlook_contact_id,
	outlook_address_id
)
VALUES
(
	@outlook_contact_id,
	@outlook_address_id
)

