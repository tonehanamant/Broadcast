CREATE PROCEDURE usp_post_aggregation_system_factors_update
(
	@id		Int,
	@name		VarChar(63),
	@system_id		Int,
	@spot_length_id		Int,
	@operator		TinyInt,
	@factor		Float,
	@start_date		DateTime,
	@end_date		DateTime
)
AS
UPDATE post_aggregation_system_factors SET
	name = @name,
	system_id = @system_id,
	spot_length_id = @spot_length_id,
	operator = @operator,
	factor = @factor,
	start_date = @start_date,
	end_date = @end_date
WHERE
	id = @id

