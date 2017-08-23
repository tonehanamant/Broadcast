CREATE PROCEDURE usp_property_histories_update
(
	@property_id		Int,
	@start_date		DateTime,
	@name		VarChar(63),
	@value		VarChar(255),
	@end_date		DateTime
)
AS
UPDATE property_histories SET
	name = @name,
	value = @value,
	end_date = @end_date
WHERE
	property_id = @property_id AND
	start_date = @start_date
