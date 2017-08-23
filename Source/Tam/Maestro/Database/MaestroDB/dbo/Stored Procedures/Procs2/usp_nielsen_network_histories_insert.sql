CREATE PROCEDURE usp_nielsen_network_histories_insert
(
	@nielsen_network_id		Int,
	@start_date		DateTime,
	@network_rating_category_id		Int,
	@nielsen_id		Int,
	@code		VarChar(15),
	@name		VarChar(63),
	@active		Bit,
	@end_date		DateTime
)
AS
INSERT INTO nielsen_network_histories
(
	nielsen_network_id,
	start_date,
	network_rating_category_id,
	nielsen_id,
	code,
	name,
	active,
	end_date
)
VALUES
(
	@nielsen_network_id,
	@start_date,
	@network_rating_category_id,
	@nielsen_id,
	@code,
	@name,
	@active,
	@end_date
)

