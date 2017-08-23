CREATE PROCEDURE usp_REL_GetProductIDsForTrafficOrder
(
                @traffic_id int
)
AS
declare @is_married int;

select 
                @is_married = case when pp.id is null then 0 else 1 end
from
                traffic t WITH (NOLOCK)
                join traffic_proposals tp WITH (NOLOCK) on tp.traffic_id = t.id
                join proposals p WITH (NOLOCK) on p.id = tp.proposal_id
                join proposal_proposals pp WITH (NOLOCK) on pp.parent_proposal_id = p.id
where
                t.id = @traffic_id

IF(@is_married is null OR @is_married = 0)
BEGIN
                select 
                                p.product_id
                from traffic t WITH (NOLOCK)
                                join traffic_proposals tp WITH (NOLOCK) on tp.traffic_id = t.id
                                join proposals p WITH (NOLOCK) on p.id = tp.proposal_id
                where
                                t.id = @traffic_id
END
ELSE
BEGIN
                select 
                                pc.product_id
                from traffic t WITH (NOLOCK)
                                join traffic_proposals tp WITH (NOLOCK) on tp.traffic_id = t.id
                                join proposals p WITH (NOLOCK) on p.id = tp.proposal_id
                                join proposal_proposals pp WITH (NOLOCK) on pp.parent_proposal_id = p.id
                                join proposals pc on pc.id = pp.child_proposal_id
                where
                                t.id = @traffic_id

END
