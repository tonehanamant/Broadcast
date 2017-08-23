



CREATE Procedure [dbo].[usp_REC_GetAllMSOs]

AS

	select 
	businesses.id, 
	businesses.code, 
	businesses.name, 
	businesses.type, 
	businesses.active, 
	businesses.effective_date
		from 
			businesses (NOLOCK)
		where 
			businesses.active = 1
		order by businesses.name

