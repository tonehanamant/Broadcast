CREATE PROCEDURE usp_cmw_contact_infos_update
(
	@id		Int,
	@cmw_contact_id		Int,
	@contact_method_id		Int,
	@value		VarChar(255),
	@ordinal		TinyInt,
	@date_last_modified		DateTime,
	@date_created		DateTime,
	@name		VarChar(63)
)
AS
UPDATE cmw_contact_infos SET
	cmw_contact_id = @cmw_contact_id,
	contact_method_id = @contact_method_id,
	value = @value,
	ordinal = @ordinal,
	date_last_modified = @date_last_modified,
	date_created = @date_created,
	name = @name
WHERE
	id = @id

