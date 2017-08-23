
CREATE PROCEDURE [dbo].[usp_ARS_GetNetworkTrafficDayparts]
      @nielsen_network_id INT
AS
BEGIN
      -- SET NOCOUNT ON added to prevent extra result sets from
      -- interfering with SELECT statements.
      SET NOCOUNT ON;
 
      SELECT
            network_traffic_dayparts.nielsen_network_id,
            network_traffic_dayparts.daypart_id,
            network_traffic_dayparts.effective_date
      FROM
            network_traffic_dayparts
      WHERE
            network_traffic_dayparts.nielsen_network_id=@nielsen_network_id
END
