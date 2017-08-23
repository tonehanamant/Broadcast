


CREATE Procedure [dbo].[usp_TCS_GetVersionNumbers]
      (
            @id Int
      )

AS

declare
      @OTID as int;

select @OTID = traffic.original_traffic_id from traffic (NOLOCK) where traffic.id = @id 

IF(@OTID IS NOT NULL)
BEGIN
      with uvw_traffic(id, original_traffic_id, revision)
      as 
      (select id, isnull(original_traffic_id, id) original_traffic_id, revision from traffic (NOLOCK) where status_id <> 24)
      SELECT DISTINCT 
		traffic.revision, 
		id
      FROM 
		uvw_traffic traffic 
      WHERE 
		traffic.original_traffic_id = @OTID
      order by 
		traffic.revision 
	  desc 
END;
ELSE
BEGIN
      with uvw_traffic(id, original_traffic_id, revision)
      as 
      (select id, isnull(original_traffic_id, id) original_traffic_id, revision from traffic (NOLOCK) where status_id <> 24)
      SELECT DISTINCT 
		traffic.revision, 
		id
      FROM 
		uvw_traffic traffic 
      WHERE 
		traffic.original_traffic_id = @id
      order by 
		traffic.revision 
	  desc 
END;


