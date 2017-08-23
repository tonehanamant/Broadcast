

 

 

-- =============================================

-- Author:                       <Author,,Name>

-- Create date: <Create Date,,>

-- Description:    <Description,,>

-- =============================================

CREATE PROCEDURE [dbo].[usp_TCS_GetTrafficIndexSpotLengths]

(

            @media_month_id as int

)

AS

BEGIN

 

SELECT DISTINCT traffic_index_values.spot_length_id, spot_lengths.length, spot_lengths.delivery_multiplier, spot_lengths.order_by, spot_lengths.is_default 

from traffic_index_values (NOLOCK) join spot_lengths (NOLOCK) on traffic_index_values.spot_length_id = spot_lengths.id where 

traffic_index_values.media_month_id = @media_month_id

 

END

