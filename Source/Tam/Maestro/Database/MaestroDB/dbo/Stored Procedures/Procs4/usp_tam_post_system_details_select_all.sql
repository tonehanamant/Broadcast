CREATE PROCEDURE usp_tam_post_system_details_select_all
AS
SELECT
	*
FROM
	tam_post_system_details WITH(NOLOCK)
