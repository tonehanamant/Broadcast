CREATE PROCEDURE usp_properties_insert
(
	@id		Int		OUTPUT,
	@name		VarChar(63),
	@value		VarChar(255),
	@effective_date		DateTime
)
AS
INSERT INTO properties
(
	name,
	value,
	effective_date
)
VALUES
(
	@name,
	@value,
	@effective_date
)

SELECT
	@id = SCOPE_IDENTITY()

