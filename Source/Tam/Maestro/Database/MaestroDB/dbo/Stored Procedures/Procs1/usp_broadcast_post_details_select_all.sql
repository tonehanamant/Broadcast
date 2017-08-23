CREATE PROCEDURE [dbo].[usp_broadcast_post_details_select_all]
AS
	SELECT
		*
	FROM
		dbo.broadcast_post_details WITH(NOLOCK)
