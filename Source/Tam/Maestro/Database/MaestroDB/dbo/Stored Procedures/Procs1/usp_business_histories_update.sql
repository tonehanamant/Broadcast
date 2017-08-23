CREATE PROCEDURE usp_business_histories_update
(
	@business_id		Int,
	@start_date		DateTime,
	@code		VarChar(15),
	@name		VarChar(63),
	@type		VarChar(15),
	@active		Bit,
	@end_date		DateTime
)
AS
UPDATE business_histories SET
	code = @code,
	name = @name,
	type = @type,
	active = @active,
	end_date = @end_date
WHERE
	business_id = @business_id AND
	start_date = @start_date
