
/*=========================================================================================
-- Author:		Rich Sara
-- Create date: 8/24/2010
-- Description:	Pull-down list for posting plans in post-logs

	- ModID :  
	- Narrative for changes made: 
	-  1:  

To test SP:
	rs.usp_SRS_GetPostingPlans_Names_for_a_Media_Month '0110' 
========================================================================================== */

create PROCEDURE [rs].[usp_SRS_GetPostingPlans_Names_for_a_Media_Month]
	@media_month varchar(4)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
SET NOCOUNT ON 
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED

	select 
		Distinct
		cast(p.id as varchar) id,
		CASE WHEN pr.name IS NOT NULL 
			 THEN
				pr.name + ' :' + cast(sl.length as varchar) + ', PlanName : ' + pr.name + ',  PlanID :' + 
				CAST(p.id AS VARCHAR)  + ', Ver# :' + dbo.GetProposalVersionIdentifier(p.id)
			 ELSE 
				p.print_title + ': ' + cast(sl.length as varchar) + ',  PlanID :' + CAST(p.id AS VARCHAR) + ', Ver# :' + dbo.GetProposalVersionIdentifier(p.id) 
			 END 
		as PostingPlan
	from 
		proposals p   
		JOIN spot_lengths sl  ON sl.id=p.default_spot_length_id
		LEFT JOIN products pr  ON pr.id=p.product_id
	Where 		 
		p.id in (select id from udf_GetValidPostingPlansForMediaMonth(@media_month))
	Order by 2
	
End