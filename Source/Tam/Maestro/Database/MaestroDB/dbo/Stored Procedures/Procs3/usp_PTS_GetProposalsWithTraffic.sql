CREATE PROCEDURE [dbo].[usp_PTS_GetProposalsWithTraffic]

AS

SELECT proposals.id, proposals.name FROM proposals where proposals.id in (SELECT distinct proposal_id from traffic_proposals)
 ORDER BY proposals.id, proposals.name
