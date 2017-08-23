
-- =============================================
-- Author:		David Sisson
-- Create date: 03/16/2009
-- Description:	Returns the id of the released traffic order related to @idTrafficTarget.
-- =============================================
CREATE FUNCTION [dbo].[udf_FindReleasedTraffic]
(	
	@idTrafficTarget as int
)
RETURNS TABLE
AS
RETURN
(
	select
		@idTrafficTarget target_traffic_id,
		t.id released_traffic_id,
		isnull(t.original_traffic_id, t.id) root_traffic_id,
		t.revision released_traffic_revision
	from
		traffic t with(nolock)
		join statuses s with(nolock) on
			s.id = t.status_id
	where
		isnull(t.original_traffic_id, t.id) in (
			select
				isnull(original_traffic_id, id)
			from
				traffic with(nolock)
			where
				@idTrafficTarget = id
		)
		and
		'Release Order' = s.name
);
