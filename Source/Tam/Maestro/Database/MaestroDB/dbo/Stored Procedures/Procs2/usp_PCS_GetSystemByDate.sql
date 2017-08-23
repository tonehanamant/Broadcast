

/***************************************************************************************************
** Date         Author          Description   
** ---------    ----------		-----------------------------------------------------
** XXXX			Stephen DeFusco
** 03/13/2017	Abdul Sukkur	Added new columns to systems 
** 05/11/2017	Abdul Sukkur	Added column custom_traffic_system to systems 
*****************************************************************************************************/
CREATE PROCEDURE usp_PCS_GetSystemByDate
	@effective_date DATETIME
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT
		system_id,
		code,
		[name],
		location,
		spot_yield_weight,
		traffic_order_format,
		flag,
		active,
		start_date,
		s.generate_traffic_alert_excel,
		s.one_advertiser_per_traffic_alert,
		s.cancel_recreate_order_traffic_alert,
		s.order_regeneration_traffic_alert,
		s.custom_traffic_system   
	FROM
		uvw_system_universe s (NOLOCK)
	WHERE
		s.start_date<=@effective_date AND (s.end_date>=@effective_date OR s.end_date IS NULL)
END
