CREATE PROCEDURE [dbo].[usp_TCS_traffic_select_by_traffic_id]
(@id as int)
AS
SELECT distinct traffic.id, traffic.display_name
FROM traffic
inner join 
	traffic_proposals a1 ON traffic.id = a1.traffic_id
inner join 
	traffic_proposals a2 ON a2.proposal_id = a1.proposal_id
where a2.traffic_id = @id
