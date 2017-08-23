-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- Changed on:	6/6/14
-- Changed By:	Brenton L Reeder
-- Changes:		Added language_id and affilaited_network_id to output
-- =============================================
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectDisplayNetworksForZoneByDate]
	@active bit,
	@effective_date datetime,
	@zone_id int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	(
		SELECT	
			n.network_id,
			n.code,
			n.name,
			n.active,
			n.flag,
			n.start_date,
			n.end_date,
			dbo.GetSubscribersByZoneAndNetwork(zn.zone_id,zn.network_id,@effective_date),
			n.language_id,
			n.affiliated_network_id,
			n.network_type_id
		FROM 
			uvw_zonenetwork_universe zn (NOLOCK)
			JOIN uvw_network_universe n (NOLOCK) ON n.network_id=zn.network_id AND (n.start_date<=@effective_date AND (n.end_date>=@effective_date OR n.end_date IS NULL))
		WHERE 
			(@active IS NULL OR n.active=@active)
			AND zn.zone_id=@zone_id
			AND (zn.start_date<=@effective_date AND (zn.end_date>=@effective_date OR zn.end_date IS NULL))

		UNION

		SELECT	
			n.id,
			n.code,
			n.name,
			n.active,
			n.flag,
			n.effective_date,
			null,
			dbo.GetSubscribersByZoneAndNetwork(zn.zone_id,zn.network_id,@effective_date),
			n.language_id,
			n.affiliated_network_id,
			n.network_type_id
		FROM 
			zone_networks zn (NOLOCK)
			JOIN networks n (NOLOCK) ON n.id=zn.network_id
		WHERE 
			(@active IS NULL OR n.active=@active)
			AND zn.network_id NOT IN (
				SELECT uvw_zonenetwork_universe.network_id FROM uvw_zonenetwork_universe (NOLOCK) WHERE
					(uvw_zonenetwork_universe.start_date<=@effective_date AND (uvw_zonenetwork_universe.end_date>=@effective_date OR uvw_zonenetwork_universe.end_date IS NULL))
					AND uvw_zonenetwork_universe.zone_id=@zone_id
				)
			AND zn.zone_id=@zone_id
	)
	ORDER BY code
END
