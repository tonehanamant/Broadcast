CREATE PROCEDURE usp_REL_GetLastTrafficOrderNotReleased
(
                @traffic_id int
)

AS

select 
                top 1
                t1.id, 
                t1.revision 
from 
                traffic t1 WITH (NOLOCK) join 
                traffic t2 WITH (NOLOCK) on t2.original_traffic_id = t1.original_traffic_id or t2.original_traffic_id = t1.id
where 
                t2.id = @traffic_id and t1.status_id <> 24
order by t1.revision desc