CREATE PROCEDURE usp_tam_post_dayparts_update
(
	@tam_post_id		Int,
	@daypart_id		Int,
	@ordinal		TinyInt
)
AS
UPDATE tam_post_dayparts SET
	ordinal = @ordinal
WHERE
	tam_post_id = @tam_post_id AND
	daypart_id = @daypart_id
