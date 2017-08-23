CREATE PROCEDURE [dbo].[usp_broadcast_proposal_detail_flights_select_all]
AS
SELECT
	*
FROM
	broadcast_proposal_detail_flights WITH(NOLOCK)
