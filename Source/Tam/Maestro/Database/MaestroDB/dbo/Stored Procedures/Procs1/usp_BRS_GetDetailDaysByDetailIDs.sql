

CREATE PROCEDURE [dbo].[usp_BRS_GetDetailDaysByDetailIDs]
@IDs varchar(MAX)

AS
BEGIN

	SET NOCOUNT ON;
	SELECT
		cmw_traffic_details_id,
		day_id,
		units,
		is_max	
	FROM
		cmw_traffic_detail_days
	WHERE
		cmw_traffic_details_id in (select id from dbo.SplitIntegers(@IDs))
END

