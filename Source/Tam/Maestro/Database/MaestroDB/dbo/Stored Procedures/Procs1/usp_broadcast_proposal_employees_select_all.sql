CREATE PROCEDURE [dbo].[usp_broadcast_proposal_employees_select_all]
AS
SELECT
	*
FROM
	broadcast_proposal_employees WITH(NOLOCK)


