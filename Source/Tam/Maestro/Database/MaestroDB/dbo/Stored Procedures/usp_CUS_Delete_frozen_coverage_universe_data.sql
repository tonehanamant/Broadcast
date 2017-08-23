
/***************************************************************************************************
** Date			Author			Description	
** ---------	----------		--------------------------------------------------------------------
** 03/21/2016	Abdul Sukkur	Delete all frozen coverage universe data
** 05/10/2016   Abdul Sukkur    1885-Create table to store the proposal topography to mvpd topography mapping
*****************************************************************************************************/
CREATE PROCEDURE [dbo].[usp_CUS_Delete_frozen_coverage_universe_data]
	  @media_month_id int
AS
BEGIN

SET NOCOUNT ON;

	DELETE FROM [frozen_media_months] WHERE media_month_id = @media_month_id

	DELETE FROM [frozen_businesses] WHERE media_month_id = @media_month_id
	DELETE FROM [frozen_systems] WHERE media_month_id = @media_month_id
	DELETE FROM [frozen_system_zones] WHERE media_month_id = @media_month_id
	DELETE FROM [frozen_system_groups] WHERE media_month_id = @media_month_id
	DELETE FROM [frozen_zone_businesses] WHERE media_month_id = @media_month_id
	DELETE FROM [frozen_zone_dmas] WHERE media_month_id = @media_month_id
	DELETE FROM [frozen_zone_states] WHERE media_month_id = @media_month_id
	DELETE FROM [frozen_networks] WHERE media_month_id = @media_month_id
	DELETE FROM [frozen_zones] WHERE media_month_id = @media_month_id
	DELETE FROM [frozen_zone_networks] WHERE media_month_id = @media_month_id

	DELETE FROM [frozen_zone_zones] WHERE media_month_id = @media_month_id
	DELETE FROM [frozen_topographies] WHERE media_month_id = @media_month_id
	DELETE FROM [frozen_topography_businesses] WHERE media_month_id = @media_month_id
	DELETE FROM [frozen_topography_dmas] WHERE media_month_id = @media_month_id
	DELETE FROM [frozen_topography_systems] WHERE media_month_id = @media_month_id
	DELETE FROM [frozen_topography_states] WHERE media_month_id = @media_month_id
	DELETE FROM [frozen_topography_zones] WHERE media_month_id = @media_month_id
	DELETE FROM [frozen_system_group_systems] WHERE media_month_id = @media_month_id
	DELETE FROM [frozen_topography_system_groups] WHERE media_month_id = @media_month_id
	
	DELETE FROM [frozen_dmas] WHERE media_month_id = @media_month_id
	DELETE FROM [frozen_states] WHERE media_month_id = @media_month_id

	DELETE FROM [frozen_traffic_network_map] WHERE media_month_id = @media_month_id
	DELETE FROM [frozen_primary_subscribers] WHERE media_month_id = @media_month_id
	
	DELETE FROM [frozen_proposal_mvpd_topography_map] WHERE media_month_id = @media_month_id
END