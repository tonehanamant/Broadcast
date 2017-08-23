

CREATE Procedure [dbo].[usp_REL_GetSystemAndZonesForTopography]
(
	@topography_id int,
    @date datetime
)

AS

select
       distinct system_id,
		zone_id
from
       dbo.udf_GetTrafficZoneInformationByTopographyAsOf(@topography_id, GetDate(), 1)
order by
	system_id

