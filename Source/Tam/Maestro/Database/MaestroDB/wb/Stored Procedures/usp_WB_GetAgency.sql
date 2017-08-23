CREATE PROCEDURE [wb].[usp_WB_GetAgency]
(
	@agency_id int
)
AS

select 
	wba.name,
	wba.address,
	wba.city,
	wba.state,
	wba.zipcode,
	wba.contact,
	wba.phone_number,
	wba.code
from 
	wb.wb_agencies (NOLOCK) wba
where
	wba.id = @agency_id
