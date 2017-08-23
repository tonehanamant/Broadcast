
CREATE PROCEDURE [dbo].[usp_time_zones_insert]
(
	@id		Int		OUTPUT,
	@name		VarChar(63),
	@utc_offset		Int,
	@active		Bit,
	@effective_date		DateTime
)
AS
INSERT INTO time_zones
(
	name,
	utc_offset,
	active,
	effective_date
)
VALUES
(
	@name,
	@utc_offset,
	@active,
	@effective_date
)

SELECT
	@id = SCOPE_IDENTITY()


