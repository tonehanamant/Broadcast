CREATE PROCEDURE usp_cmw_contact_infos_insert
(
	@id		Int		OUTPUT,
	@cmw_contact_id		Int,
	@contact_method_id		Int,
	@value		VarChar(255),
	@ordinal		TinyInt,
	@date_last_modified		DateTime,
	@date_created		DateTime,
	@name		VarChar(63)
)
AS
INSERT INTO cmw_contact_infos
(
	cmw_contact_id,
	contact_method_id,
	value,
	ordinal,
	date_last_modified,
	date_created,
	name
)
VALUES
(
	@cmw_contact_id,
	@contact_method_id,
	@value,
	@ordinal,
	@date_last_modified,
	@date_created,
	@name
)

SELECT
	@id = SCOPE_IDENTITY()

