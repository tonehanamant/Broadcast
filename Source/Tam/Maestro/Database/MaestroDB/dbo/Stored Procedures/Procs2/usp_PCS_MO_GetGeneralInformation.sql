
CREATE PROCEDURE [dbo].[usp_PCS_MO_GetGeneralInformation]  
	@proposal_id INT
AS  
BEGIN
	SELECT   
		'CADENT NETWORK' [trading partner code],  
		CASE WHEN p.original_proposal_id IS NULL THEN p.id ELSE p.original_proposal_id END [proposal id],  
		'NETCODE' [network advertiser code],  
		p.advertiser_company_id,  
		'AGCODE',  
		p.agency_company_id,  
		'ADVCODE',  
		e.lastname + ',' + e.firstname [AE],  
		e.phone [ae phone],  
		replace(CONVERT(VARCHAR(8), p.start_date, 2) , '.', '') [start date],  
		replace(CONVERT(VARCHAR(8), p.end_date, 2) , '.', '') [end date],  
		'B' [calendar type],  
		'C' [revenue type],  
		p.version_number [Revision],  
		round(p.total_gross_cost, 2),  
		p.total_spots,  
		0 [total billboards],  
		replace(CONVERT(VARCHAR(8), GETDATE(), 2) , '.', '') [send date],  
		replace(convert(VARCHAR(6), getdate(), 108), ':', '') [send time],  
		p.print_title [contract name]  
	FROM  
		proposals p (NOLOCK)  
		left join employees e (NOLOCK) on e.id = p.salesperson_employee_id  
	where  
		p.id = @proposal_id
END  
  
