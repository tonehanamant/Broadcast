


/***************************************************************************************************
** Date			Author			Description	
** ---------	----------		--------------------------------------------------------------------
** XX/XX/XXXX	XXXXX			Created Function
** 07/28/2015	Abdul Sukkur 	Task-8626-Statistical Tables for Married Plans to Improve Performance.Changed to get all columns in traffic_detail_audiences
*****************************************************************************************************/
CREATE Procedure [dbo].[usp_TCS_GetTrafficDetailAudiencesByDetailID]
	(
		@id Int
	)
AS
SELECT 
	*
from 
	traffic_detail_audiences (NOLOCK) 
where 
	traffic_detail_id = @id


