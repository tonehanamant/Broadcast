CREATE PROCEDURE usp_nielsen_networks_insert
(
	@id		Int		OUTPUT,
	@network_rating_category_id		Int,
	@nielsen_id		Int,
	@code		VarChar(15),
	@name		VarChar(63),
	@active		Bit,
	@effective_date		DateTime
)
AS
INSERT INTO nielsen_networks
(
	network_rating_category_id,
	nielsen_id,
	code,
	name,
	active,
	effective_date
)
VALUES
(
	@network_rating_category_id,
	@nielsen_id,
	@code,
	@name,
	@active,
	@effective_date
)

SELECT
	@id = SCOPE_IDENTITY()

