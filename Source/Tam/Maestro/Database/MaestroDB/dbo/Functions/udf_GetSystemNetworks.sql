	CREATE FUNCTION [dbo].[udf_GetSystemNetworks]
	(     
		  @system_id as int,
		  @dateAsOf as datetime
	)
	RETURNS TABLE
	AS
	RETURN 
	(
		SELECT
			n.network_id,
			n.code,
			n.name
		FROM
			uvw_zonenetwork_universe zn
			JOIN uvw_systemzone_universe sz ON sz.zone_id=zn.zone_id
				AND sz.type='BILLING'
				AND (sz.start_date<=@dateAsOf AND (sz.end_date>=@dateAsOf OR sz.end_date IS NULL))
				AND sz.system_id=@system_id
			JOIN uvw_network_universe n ON n.network_id=zn.network_id
				AND (n.start_date<=@dateAsOf AND (n.end_date>=@dateAsOf OR n.end_date IS NULL))
		WHERE
			  (zn.start_date<=@dateAsOf AND (zn.end_date>=@dateAsOf OR zn.end_date IS NULL))
		GROUP BY
			n.network_id,
			n.code,
			n.name
	);
