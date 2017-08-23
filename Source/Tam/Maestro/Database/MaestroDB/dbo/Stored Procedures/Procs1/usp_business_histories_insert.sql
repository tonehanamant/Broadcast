CREATE PROCEDURE usp_business_histories_insert
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
INSERT INTO business_histories
(
	business_id,
	start_date,
	code,
	name,
	type,
	active,
	end_date
)
VALUES
(
	@business_id,
	@start_date,
	@code,
	@name,
	@type,
	@active,
	@end_date
)

