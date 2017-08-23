CREATE PROCEDURE [dbo].[usp_broadcast_proposal_details_select_all]
AS
SELECT
	*
FROM
	broadcast_proposal_details WITH(NOLOCK)

