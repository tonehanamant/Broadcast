/****** Object:  Function [dbo].[udf_GetTrafficAmountByTrafficId]    Script Date: 02/12/2014 10:16:38 ******/
CREATE FUNCTION [dbo].[udf_GetTrafficAmountByTrafficId]
(
      @traffic_id INT
)
RETURNS MONEY
AS
BEGIN
      DECLARE @return AS MONEY;

      SELECT
            @return = SUM(td.traffic_amount)
      FROM
            dbo.traffic_details td (NOLOCK)
      WHERE
            td.traffic_id=@traffic_id;
      
      RETURN @return;
END
