-- =============================================
-- Author:        <Author,,Name>
-- Create date: <Create Date,,>
-- Description:   <Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_TCS_GetTrafficIndexes]
(
	@media_month_id as int,
	@spot_length_id as int
)
AS
BEGIN
	select 
		id, network_id, media_month_id, spot_length_id, index_value 
	from 
		traffic_index_values (NOLOCK) 
	where 
		media_month_id = @media_month_id 
		and spot_length_id = @spot_length_id
	order by 
		network_id
END