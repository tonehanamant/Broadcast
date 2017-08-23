CREATE PROCEDURE [dbo].[usp_time_zones_update]
(
	@id		Int,
	@name		VarChar(63),
	@utc_offset		Int,
	@active		Bit,
	@effective_date		DateTime
)
AS
UPDATE time_zones SET
	name = @name,
	utc_offset = @utc_offset,
	active = @active,
	effective_date = @effective_date
WHERE
	id = @id
