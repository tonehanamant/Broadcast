
/***************************************************************************************************
** Date			Author			Description	
** ---------	----------		--------------------------------------------------------------------
** XX/XX/XXXX	XXXXX			Created Function
** 07/28/2015	Abdul Sukkur 	Task-8626-Statistical Tables for Married Plans to Improve Performance.
*****************************************************************************************************/
CREATE FUNCTION [dbo].[udf_GetReleaseAmountByTrafficId]
(
      @traffic_id INT
)
RETURNS MONEY
AS
BEGIN
      DECLARE @return AS MONEY;

      SELECT
            @return = SUM(ISNULL(td.release_amount, 0))
      FROM
            dbo.traffic_details td (NOLOCK)
      WHERE
            td.traffic_id=@traffic_id;
      
      RETURN @return;
END
