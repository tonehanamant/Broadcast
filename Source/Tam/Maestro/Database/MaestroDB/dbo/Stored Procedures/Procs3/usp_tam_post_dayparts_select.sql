CREATE PROCEDURE usp_tam_post_dayparts_select
(
	@tam_post_id		Int,
	@daypart_id		Int
)
AS
SELECT
	*
FROM
	tam_post_dayparts WITH(NOLOCK)
WHERE
	tam_post_id=@tam_post_id
	AND
	daypart_id=@daypart_id

