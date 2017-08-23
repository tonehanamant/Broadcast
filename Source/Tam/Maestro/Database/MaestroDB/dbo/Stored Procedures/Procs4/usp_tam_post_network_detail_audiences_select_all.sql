CREATE PROCEDURE usp_tam_post_network_detail_audiences_select_all
AS
SELECT
	*
FROM
	tam_post_network_detail_audiences WITH(NOLOCK)
