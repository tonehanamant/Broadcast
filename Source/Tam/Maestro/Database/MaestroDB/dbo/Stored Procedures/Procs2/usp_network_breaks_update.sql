CREATE PROCEDURE usp_network_breaks_update
(
	@id		Int,
	@nielsen_network_id		Int,
	@seconds_after_hour		Int,
	@length		Int,
	@effective_date		DateTime,
	@active		Bit
)
AS
UPDATE network_breaks SET
	nielsen_network_id = @nielsen_network_id,
	seconds_after_hour = @seconds_after_hour,
	length = @length,
	effective_date = @effective_date,
	active = @active
WHERE
	id = @id

