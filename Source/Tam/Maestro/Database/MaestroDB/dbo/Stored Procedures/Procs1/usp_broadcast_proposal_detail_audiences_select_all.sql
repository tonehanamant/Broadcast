CREATE PROCEDURE [dbo].[usp_broadcast_proposal_detail_audiences_select_all]
AS
SELECT
	*
FROM
	broadcast_proposal_detail_audiences WITH(NOLOCK)

