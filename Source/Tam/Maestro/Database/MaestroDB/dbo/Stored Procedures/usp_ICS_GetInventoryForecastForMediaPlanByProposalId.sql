-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 2/26/2016
-- Description:	Returns lastest inventory forecast specific to the dimensions of the media plan line items (networks).
-- =============================================
/*					
	EXEC usp_ICS_GetInventoryForecastForMediaPlanByProposalId 66759,0
*/
CREATE PROCEDURE [dbo].[usp_ICS_GetInventoryForecastForMediaPlanByProposalId]
	@proposal_id INT
AS
BEGIN
	SET NOCOUNT ON;
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

	DECLARE @inventory_details InventoryRequestTable
	INSERT INTO @inventory_details (media_month_id,media_week_id,network_id,daypart_id,hh_eq_cpm,contracted_subscribers) 
		SELECT
			mw.media_month_id,
			mw.id,
			pd.network_id,
			pd.daypart_id,
			ROUND(dbo.GetProposalDetailCPMEquivalized(pd.id,31),2) 'hh_eq_cpm',
			SUM(CAST(pdw.units * pd.topography_universe AS BIGINT))		
		FROM
			proposal_details pd (NOLOCK)
			JOIN proposal_detail_worksheets pdw (NOLOCK) ON pdw.proposal_detail_id=pd.id
			JOIN media_weeks mw (NOLOCK) ON mw.id=pdw.media_week_id
		WHERE
			pd.proposal_id=@proposal_id
			AND pdw.units>0
			AND pd.topography_universe>0
		GROUP BY
			mw.media_month_id,
			mw.id,
			pd.network_id,
			pd.daypart_id,
			ROUND(dbo.GetProposalDetailCPMEquivalized(pd.id,31),2)

	DECLARE @start_date DATETIME;
	SELECT @start_date = MIN(mm.start_date) FROM @inventory_details pd JOIN media_months mm ON mm.id=pd.media_month_id;
	
	DECLARE @latest_base_media_month_id SMALLINT;
	SELECT @latest_base_media_month_id = dbo.udf_GetLatestPossibleInventoryForecastBaseMediaMonthId(@start_date);

    DECLARE @inventory_filter_dayparts TABLE (source_daypart_id INT NOT NULL, component_daypart_id INT NOT NULL, component_hours FLOAT NOT NULL, total_component_hours FLOAT NOT NULL, PRIMARY KEY (source_daypart_id, component_daypart_id));
	INSERT INTO @inventory_filter_dayparts
		SELECT DISTINCT
			pd.daypart_id,
			cd.id 'component_daypart_id',
			dbo.GetIntersectingDaypartHours(d.start_time,d.end_time, dc.start_time,dc.end_time) * dbo.GetIntersectingDaypartDays(d.mon,d.tue,d.wed,d.thu,d.fri,d.sat,d.sun, dc.mon,dc.tue,dc.wed,dc.thu,dc.fri,dc.sat,dc.sun) 'component_hours',
			d.total_hours 'total_component_hours'
		FROM
			@inventory_details pd
			JOIN vw_ccc_daypart d ON d.id=pd.daypart_id
			CROSS APPLY dbo.udf_GetIntersectingInventoryComponentDayparts(d.start_time,d.end_time,d.mon,d.tue,d.wed,d.thu,d.fri,d.sat,d.sun) cd
			JOIN vw_ccc_daypart dc ON dc.id=cd.id;

	DECLARE @inventory_filter TABLE (media_month_id SMALLINT NOT NULL, media_week_id INT NOT NULL, network_id INT NOT NULL, component_daypart_id INT NOT NULL, PRIMARY KEY (media_month_id ASC, media_week_id ASC, network_id ASC, component_daypart_id ASC));
	INSERT INTO @inventory_filter
		SELECT
			pd.media_month_id,
			pd.media_week_id,
			pd.network_id,
			ifd.component_daypart_id
		FROM
			@inventory_details pd
			JOIN @inventory_filter_dayparts ifd ON ifd.source_daypart_id=pd.daypart_id
		GROUP BY
			pd.media_month_id,
			pd.media_week_id,
			pd.network_id,
			ifd.component_daypart_id;
	
	SELECT mm.* FROM media_months mm WHERE mm.id=@latest_base_media_month_id;

	SELECT
		inv.forecast_media_month_id 'ForecastMediaMonthId',
		inv.forecast_media_week_id 'ForecastMediaWeekId',
		inv.network_id 'NetworkId',
		inv.component_daypart_id 'ComponentDaypartId',
		inv.hh_eq_cpm_start 'HhEqCpmStart',
		inv.hh_eq_cpm_end 'HhEqCpmEnd',
		SUM(inv.subscribers) 'Subscribers'
	FROM
		dbo.inventory_forecasts inv
		JOIN @inventory_filter invf ON invf.media_month_id=inv.forecast_media_month_id
			AND invf.network_id=inv.network_id
			AND invf.media_week_id=inv.forecast_media_week_id
			AND invf.component_daypart_id=inv.component_daypart_id
	WHERE
		inv.base_media_month_id=@latest_base_media_month_id
	GROUP BY
		inv.forecast_media_month_id,
		inv.forecast_media_week_id,
		inv.network_id,
		inv.component_daypart_id,
		inv.hh_eq_cpm_start,
		inv.hh_eq_cpm_end
END