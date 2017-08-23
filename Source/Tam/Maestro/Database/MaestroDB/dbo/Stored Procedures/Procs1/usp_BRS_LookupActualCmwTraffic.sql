-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 10/7/2011
-- Description:	<Description,,>
-- =============================================
-- usp_BRS_LookupActualCmwTraffic 2359
CREATE PROCEDURE usp_BRS_LookupActualCmwTraffic
	@cmw_traffic_id INT
AS
BEGIN
	DECLARE @max_version INT
	DECLARE @actual_cmw_traffic_id INT
	DECLARE @actual_original_cmw_traffic_id INT

	SELECT 
		@max_version = MAX(ct.version_number),
		@actual_original_cmw_traffic_id = ct.original_cmw_traffic_id,
		@actual_cmw_traffic_id = ct.id
	FROM 
		cmw_traffic ct (NOLOCK) 
	WHERE 
		ct.original_cmw_traffic_id=@cmw_traffic_id
	GROUP BY
		ct.original_cmw_traffic_id,
		ct.id
		
	IF @actual_cmw_traffic_id IS NULL
		SET @actual_cmw_traffic_id = @cmw_traffic_id
		
	SELECT
		ct.*
	FROM
		cmw_traffic ct (NOLOCK)
	WHERE
		ct.id=@actual_cmw_traffic_id
END
