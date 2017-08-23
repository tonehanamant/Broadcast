CREATE PROCEDURE usp_tam_post_network_details_select_all
AS
SELECT
	*
FROM
	tam_post_network_details WITH(NOLOCK)
