CREATE PROCEDURE [dbo].[usp_BRS_GetDisplayNameByOrderID]
@OrderID int
AS
BEGIN

      SET NOCOUNT ON;

      SELECT 
            id,
            CASE 
                  WHEN original_cmw_traffic_id IS NULL 
                  THEN CAST(cmw_traffic.id AS varchar) 
                  ELSE CAST(original_cmw_traffic_id AS varchar) + '-R' + CAST(version_number AS varchar) 
            END AS display_id 
      FROM
            cmw_traffic (nolock) 
      WHERE
            id = @OrderID
END