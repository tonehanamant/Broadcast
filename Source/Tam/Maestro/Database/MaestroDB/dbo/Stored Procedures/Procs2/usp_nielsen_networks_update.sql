CREATE PROCEDURE usp_nielsen_networks_update
(
	@id		Int,
	@network_rating_category_id		Int,
	@nielsen_id		Int,
	@code		VarChar(15),
	@name		VarChar(63),
	@active		Bit,
	@effective_date		DateTime
)
AS
UPDATE nielsen_networks SET
	network_rating_category_id = @network_rating_category_id,
	nielsen_id = @nielsen_id,
	code = @code,
	name = @name,
	active = @active,
	effective_date = @effective_date
WHERE
	id = @id

