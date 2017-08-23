CREATE PROCEDURE usp_property_histories_insert
(
	@property_id		Int,
	@start_date		DateTime,
	@name		VarChar(63),
	@value		VarChar(255),
	@end_date		DateTime
)
AS
INSERT INTO property_histories
(
	property_id,
	start_date,
	name,
	value,
	end_date
)
VALUES
(
	@property_id,
	@start_date,
	@name,
	@value,
	@end_date
)

