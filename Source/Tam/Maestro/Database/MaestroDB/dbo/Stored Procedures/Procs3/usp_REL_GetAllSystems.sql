

CREATE PROCEDURE [dbo].[usp_REL_GetAllSystems]
	
AS

select id, code, name, location, spot_yield_weight, traffic_order_format, flag, active, effective_date 
 from systems (NOLOCK) order by code

