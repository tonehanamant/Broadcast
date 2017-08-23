CREATE PROCEDURE [dbo].[usp_REL_GetTrafficDetailsForSuspend]
	@traffic_id int
AS
BEGIN
	DECLARE @sales_model_id int

	SELECT 
		@sales_model_id = psm.sales_model_id
	FROM 
		traffic t
		JOIN traffic_proposals tp on t.id = tp.traffic_id
		JOIN proposals p on tp.proposal_id = p.id
		JOIN proposal_sales_models psm on p.id = psm.proposal_id
	WHERE 
		t.id = @traffic_id

	IF (@sales_model_id = 4 OR @sales_model_id = 5) -- National Direct Response OR IMW
	BEGIN -- we need to include all topography types, including ADS
		SELECT 
			traffic_details.id, 
			networks.code, 
			traffic_details.network_id, 
			traffic_detail_topographies.daypart_id, 
			traffic_detail_weeks.suspended,
			traffic_detail_weeks.start_date, 
			traffic_detail_weeks.end_date,
			cast(sum(traffic_detail_topographies.spots) as float)
		FROM 
			traffic_details WITH (NOLOCK)
			JOIN traffic WITH (NOLOCK) on traffic.id = traffic_details.traffic_id
			join traffic_detail_weeks WITH (NOLOCK) on traffic_details.id = traffic_detail_weeks.traffic_detail_id
			join traffic_detail_topographies WITH (NOLOCK) on traffic_detail_weeks.id = traffic_detail_topographies.traffic_detail_week_id 
			JOIN uvw_network_universe networks WITH (NOLOCK)  ON networks.network_id=traffic_details.network_id AND (networks.start_date<=traffic.start_date AND (networks.end_date>=traffic.start_date OR networks.end_date IS NULL))
			JOIN topographies WITH (NOLOCK) on traffic_detail_topographies.topography_id = topographies.id
		WHERE 
			traffic_details.traffic_id = @traffic_id 
		GROUP BY 
			traffic_details.id, 
			networks.code, 
			traffic_details.network_id, 
			traffic_detail_topographies.daypart_id, 
			traffic_detail_weeks.suspended,
			traffic_detail_weeks.start_date, 
			traffic_detail_weeks.end_date
		ORDER BY
			networks.code,
			traffic_detail_weeks.start_date
	END
	ELSE
	BEGIN -- we want to exclude ADS from the results
		SELECT 
			traffic_details.id, 
			networks.code, 
			traffic_details.network_id, 
			traffic_detail_topographies.daypart_id, 
			traffic_detail_weeks.suspended,
			traffic_detail_weeks.start_date, 
			traffic_detail_weeks.end_date,
			cast(sum(traffic_detail_topographies.spots) as float)
		FROM 
			traffic_details WITH (NOLOCK)
			JOIN traffic WITH (NOLOCK) on traffic.id = traffic_details.traffic_id
			join traffic_detail_weeks WITH (NOLOCK) on traffic_details.id = traffic_detail_weeks.traffic_detail_id
			join traffic_detail_topographies WITH (NOLOCK) on traffic_detail_weeks.id = traffic_detail_topographies.traffic_detail_week_id 
			JOIN uvw_network_universe networks WITH (NOLOCK) ON networks.network_id=traffic_details.network_id AND (networks.start_date<=traffic.start_date AND (networks.end_date>=traffic.start_date OR networks.end_date IS NULL))
			JOIN topographies WITH (NOLOCK) on traffic_detail_topographies.topography_id = topographies.id
		WHERE 
			traffic_details.traffic_id = @traffic_id 
			AND topographies.topography_type = 0 -- filter out ADS
		GROUP BY 
			traffic_details.id, 
			networks.code, 
			traffic_details.network_id, 
			traffic_detail_topographies.daypart_id, 
			traffic_detail_weeks.suspended,
			traffic_detail_weeks.start_date, 
			traffic_detail_weeks.end_date
		ORDER BY
			networks.code,
			traffic_detail_weeks.start_date
	END
END
