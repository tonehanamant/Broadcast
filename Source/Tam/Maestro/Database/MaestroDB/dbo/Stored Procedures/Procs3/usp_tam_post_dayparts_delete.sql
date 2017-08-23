CREATE PROCEDURE usp_tam_post_dayparts_delete
(
	@tam_post_id		Int,
	@daypart_id		Int)
AS
DELETE FROM
	tam_post_dayparts
WHERE
	tam_post_id = @tam_post_id
 AND
	daypart_id = @daypart_id
