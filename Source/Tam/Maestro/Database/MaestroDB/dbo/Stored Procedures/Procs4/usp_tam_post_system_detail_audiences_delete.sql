CREATE PROCEDURE usp_tam_post_system_detail_audiences_delete
(
	@tam_post_system_detail_id		Int,
	@audience_id		Int)
AS
DELETE FROM
	tam_post_system_detail_audiences
WHERE
	tam_post_system_detail_id = @tam_post_system_detail_id
 AND
	audience_id = @audience_id
