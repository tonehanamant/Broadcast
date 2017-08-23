CREATE PROCEDURE usp_tam_post_dayparts_select_all
AS
SELECT
	*
FROM
	tam_post_dayparts WITH(NOLOCK)
