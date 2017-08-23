CREATE PROCEDURE usp_tam_post_dayparts_insert
(
	@tam_post_id		Int,
	@daypart_id		Int,
	@ordinal		TinyInt
)
AS
INSERT INTO tam_post_dayparts
(
	tam_post_id,
	daypart_id,
	ordinal
)
VALUES
(
	@tam_post_id,
	@daypart_id,
	@ordinal
)

