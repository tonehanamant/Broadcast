
/***************************************************************************************************
** Date         Author          Description   
** ---------    ----------		-----------------------------------------------------
** XXXX			Stephen DeFusco
** 03/13/2017	Abdul Sukkur	Added new columns to systems 
** 05/11/2017	Abdul Sukkur	Added column custom_traffic_system to systems 
*****************************************************************************************************/
CREATE PROCEDURE [dbo].[usp_STS2_GetSystemByDate]
	@system_id INT,
	@effective_date DATETIME
AS
BEGIN
	IF (SELECT COUNT(1) FROM uvw_system_universe s WHERE s.start_date<=@effective_date AND (s.end_date>=@effective_date OR s.end_date IS NULL) AND s.system_id=@system_id) > 0
	BEGIN
		SELECT
			s.system_id,
			s.code,
			s.name,
			s.location,
			s.spot_yield_weight,
			s.traffic_order_format,
			s.flag,
			s.active,
			s.start_date,
			s.generate_traffic_alert_excel,
			s.one_advertiser_per_traffic_alert,
			s.cancel_recreate_order_traffic_alert,
			s.order_regeneration_traffic_alert,
			s.custom_traffic_system
		FROM
			uvw_system_universe s
		WHERE
			s.start_date<=@effective_date AND (s.end_date>=@effective_date OR s.end_date IS NULL)
			AND s.system_id=@system_id
	END
	ELSE
	BEGIN
		SELECT
			s.*
		FROM
			systems s (NOLOCK)
		WHERE
			s.id=@system_id
	END
END
