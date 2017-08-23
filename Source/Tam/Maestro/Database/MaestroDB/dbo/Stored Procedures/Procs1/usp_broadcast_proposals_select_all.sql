CREATE PROCEDURE [dbo].[usp_broadcast_proposals_select_all]
AS
SELECT
	*
FROM
	broadcast_proposals WITH(NOLOCK)

