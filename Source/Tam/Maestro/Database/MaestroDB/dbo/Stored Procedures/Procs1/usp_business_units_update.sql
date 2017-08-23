CREATE PROCEDURE [dbo].[usp_business_units_update]
(
	@id		TinyInt,
	@name		VarChar(50),
	@short_name		VarChar(20),
	@abbreviation		VarChar(10),
	@employee_id		Int
)
AS
UPDATE business_units SET
	name = @name,
	short_name = @short_name,
	abbreviation = @abbreviation,
	employee_id = @employee_id
WHERE
	id = @id
