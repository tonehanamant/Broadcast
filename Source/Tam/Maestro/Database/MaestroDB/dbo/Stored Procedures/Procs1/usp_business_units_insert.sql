CREATE PROCEDURE [dbo].[usp_business_units_insert]
(
	@id		TinyInt		OUTPUT,
	@name		VarChar(50),
	@short_name		VarChar(20),
	@abbreviation		VarChar(10),
	@employee_id		Int
)
AS
INSERT INTO business_units
(
	name,
	short_name,
	abbreviation,
	employee_id
)
VALUES
(
	@name,
	@short_name,
	@abbreviation,
	@employee_id
)
SELECT
	@id = SCOPE_IDENTITY()
