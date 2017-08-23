CREATE Procedure [dbo].[usp_REL2_GetNetworksFromTrafficDetails]
		@traffic_id int
AS
BEGIN
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
			SELECT DISTINCT
				id = networks.[network_id]
				,networks.[code]
				,networks.[name]
				,networks.[active]
				,networks.[flag]
				,effective_date = networks.[start_date]
				,networks.language_id
				,networks.affiliated_network_id
				,networks.network_type_id
			FROM 
					traffic_details WITH (NOLOCK)
					JOIN traffic WITH (NOLOCK) on traffic.id = traffic_details.traffic_id
					JOIN uvw_network_universe networks WITH (NOLOCK)  ON networks.network_id=traffic_details.network_id 
						AND (networks.start_date<=traffic.start_date 
						AND (networks.end_date>=traffic.start_date OR networks.end_date IS NULL))
			WHERE 
					traffic_details.traffic_id = @traffic_id 
END
