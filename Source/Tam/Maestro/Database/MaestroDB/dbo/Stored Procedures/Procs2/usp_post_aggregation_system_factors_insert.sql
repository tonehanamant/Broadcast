CREATE PROCEDURE usp_post_aggregation_system_factors_insert
(
	@id		Int		OUTPUT,
	@name		VarChar(63),
	@system_id		Int,
	@spot_length_id		Int,
	@operator		TinyInt,
	@factor		Float,
	@start_date		DateTime,
	@end_date		DateTime
)
AS
INSERT INTO post_aggregation_system_factors
(
	name,
	system_id,
	spot_length_id,
	operator,
	factor,
	start_date,
	end_date
)
VALUES
(
	@name,
	@system_id,
	@spot_length_id,
	@operator,
	@factor,
	@start_date,
	@end_date
)

SELECT
	@id = SCOPE_IDENTITY()

