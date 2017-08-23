CREATE PROCEDURE usp_network_breaks_insert
(
	@id		Int		OUTPUT,
	@nielsen_network_id		Int,
	@seconds_after_hour		Int,
	@length		Int,
	@effective_date		DateTime,
	@active		Bit
)
AS
INSERT INTO network_breaks
(
	nielsen_network_id,
	seconds_after_hour,
	length,
	effective_date,
	active
)
VALUES
(
	@nielsen_network_id,
	@seconds_after_hour,
	@length,
	@effective_date,
	@active
)

SELECT
	@id = SCOPE_IDENTITY()

