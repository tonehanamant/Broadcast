CREATE PROCEDURE [dbo].[usp_broadcast_proposal_statuses_select_all]
AS
SELECT
	*
FROM
	broadcast_proposal_statuses WITH(NOLOCK)

