

CREATE PROCEDURE usp_BS_GetBroadcastDaypart 
(
	@daypart_id int
)
AS
	SELECT *
from
	broadcast_dayparts bd with (NOLOCK)
where
	bd.id = @daypart_id
