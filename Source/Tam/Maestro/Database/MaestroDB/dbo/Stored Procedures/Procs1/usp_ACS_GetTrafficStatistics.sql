-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_ACS_GetTrafficStatistics]
	@traffic_id INT,
	@system_id INT,
	@media_month_id INT
AS
BEGIN
	SELECT
		CASE WHEN (SELECT COUNT(*) FROM traffic_proposals (NOLOCK) WHERE traffic_id=@traffic_id AND proposal_id IN (SELECT id FROM proposals (NOLOCK) WHERE is_audience_deficiency_unit_schedule=1)) > 0 THEN 1 ELSE 0 END 'is_adu',
		dbo.GetTotalSpotsTrafficked(@traffic_id,@system_id,@media_month_id,traffic.start_date) 'total_spots_trafficked',
		dbo.GetTotalAffidavitsWithTrafficId(@traffic_id,@system_id,@media_month_id) 'total_spots'
	FROM
		traffic (NOLOCK) 
	WHERE
		id=@traffic_id
END
