CREATE PROCEDURE usp_nielsen_network_histories_update
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
UPDATE nielsen_network_histories SET
	network_rating_category_id = @network_rating_category_id,
	nielsen_id = @nielsen_id,
	code = @code,
	name = @name,
	active = @active,
	end_date = @end_date
WHERE
	nielsen_network_id = @nielsen_network_id AND
	start_date = @start_date
