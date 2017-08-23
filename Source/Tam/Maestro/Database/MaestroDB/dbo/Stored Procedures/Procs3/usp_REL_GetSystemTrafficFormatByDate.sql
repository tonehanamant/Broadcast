
/***************************************************************************************************
** Date         Author          Description   
** ---------    ----------		-----------------------------------------------------
** 4/8/2010		Stephen DeFusco
** 12/18/2013	XXX				Added use_net_pdfs (bit 25) and modified SystemTrafficInfo and TrafficOrderFormatFlag accordingly
** 03/20/2017	Abdul Sukkur	Added generate_traffic_alert_excel, one_advertiser_per_traffic_alert,cancel_recreate_order_traffic_alert & order_regeneration_traffic_alert
*****************************************************************************************************/
-- usp_REL_GetSystemTrafficFormatByDate 392,'12/17/2013'
CREATE PROCEDURE [dbo].[usp_REL_GetSystemTrafficFormatByDate]
	@system_id INT,
	@effective_date DATETIME
AS
BEGIN
	SELECT
		s.code, 
		s.name, 
		s.location, 
		s.spot_yield_weight, 
		CASE s.traffic_order_format & 0x000003E0
			WHEN 0x00000020 THEN 'Standard'
			WHEN 0x00000040 THEN 'Spot-Rate'
			WHEN 0x00000060 THEN 'DP Spot-Rate'
			ELSE 'Unknown'
		END 'system_order_type',
		CASE s.traffic_order_format & 0x00000800
			WHEN 0x00000800 THEN 1
			ELSE 0
		END 'system_xml_order',
		CASE s.traffic_order_format & 0x00000400
			WHEN 0x00000400 THEN 1
			ELSE 0
		END 'system_pdf_order',
		CASE 
			WHEN s.traffic_order_format & 0x00001000 = 0x00001000 THEN 1
			WHEN s.traffic_order_format & 0x00001000 <> 0x00001000 THEN 0
			ELSE -1
		END 'system_confirmation',
		CASE 
			WHEN s.traffic_order_format & 0x00002000 = 0x00002000 THEN 1
			WHEN s.traffic_order_format & 0x00002000 <> 0x00002000 THEN 0
			ELSE -1
		END 'system_lineup',
		CASE 
			WHEN s.traffic_order_format & 0x00004000 = 0x00004000 THEN 1
			WHEN s.traffic_order_format & 0x00004000 <> 0x00004000 THEN 0
			ELSE -1
		END 'system_alerts',
		CASE 
			WHEN s.traffic_order_format & 0x00008000 = 0x00008000 THEN 1
			WHEN s.traffic_order_format & 0x00008000 <> 0x00008000 THEN 0
			ELSE -1
		END 'system_orders',
		CASE 
			WHEN s.traffic_order_format & 0x00010000 = 0x00010000 THEN 1
			WHEN s.traffic_order_format & 0x00010000 <> 0x00010000 THEN 0
			ELSE -1
		END 'system_cutsheet',
		CASE s.traffic_order_format & 0x00020000
			WHEN 0x00020000 THEN 1
			ELSE 0
		END 'system_multimarket',
		CASE s.traffic_order_format & 0x00040000
			WHEN 0x00040000 THEN 1
			ELSE 0
		END 'system_store_spot_yield',
		CASE s.traffic_order_format & 0x00080000
			WHEN 0x00080000 THEN 1
			ELSE 0
		END 'system_fixed_rate',
		CASE s.traffic_order_format & 0x00200000
			WHEN 0x00200000 THEN 1
			ELSE 0
		END 'generate_spot_files',
		CASE s.traffic_order_format & 0x00400000
			WHEN 0x00400000 THEN 1
			ELSE 0
		END 'generate_rotation_files',
		CASE s.traffic_order_format & 0x00800000
			WHEN 0x00800000 THEN 1
			ELSE 0
		END 'fill_empty_weeks_in_xml',
		CASE s.traffic_order_format & 0x01000000
			WHEN 0x01000000 THEN 1
			ELSE 0
		END 'split_xml_by_media_month',
		CASE s.traffic_order_format & 0x02000000
			WHEN 0x02000000 THEN 1
			ELSE 0
		END 'use_net_pdfs',
		s.generate_traffic_alert_excel,
		s.one_advertiser_per_traffic_alert,
		s.cancel_recreate_order_traffic_alert,
		s.order_regeneration_traffic_alert
	FROM
		uvw_system_universe s (NOLOCK)
	WHERE 
		s.system_id = @system_id
		AND (s.start_date<=@effective_date AND (s.end_date>=@effective_date OR s.end_date IS NULL)) 
	ORDER BY
		s.code;
END
