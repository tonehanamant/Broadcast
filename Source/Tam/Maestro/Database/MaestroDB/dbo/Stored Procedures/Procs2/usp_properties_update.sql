CREATE PROCEDURE usp_properties_update
(
	@id		Int,
	@name		VarChar(63),
	@value		VarChar(255),
	@effective_date		DateTime
)
AS
UPDATE properties SET
	name = @name,
	value = @value,
	effective_date = @effective_date
WHERE
	id = @id

