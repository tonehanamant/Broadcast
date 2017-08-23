


-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
-- usp_PCS_GetProposalBusinessObject 21300
CREATE PROCEDURE [dbo].[usp_PCS_GetProposalBusinessObject]
	@proposal_id INT
AS
BEGIN
	SET NOCOUNT ON;

    SELECT * FROM proposals						(NOLOCK) WHERE id=@proposal_id
	SELECT * FROM proposal_flights				(NOLOCK) WHERE proposal_id=@proposal_id
	SELECT * FROM proposal_audiences			(NOLOCK) WHERE proposal_id=@proposal_id 
	SELECT * FROM proposal_employees			(NOLOCK) WHERE proposal_id=@proposal_id
	SELECT * FROM proposal_contacts				(NOLOCK) WHERE proposal_id=@proposal_id
	SELECT * FROM proposal_details				(NOLOCK) WHERE proposal_id=@proposal_id
	SELECT * FROM proposal_detail_audiences		(NOLOCK) WHERE proposal_detail_id IN (SELECT id FROM proposal_details (NOLOCK) WHERE proposal_id=@proposal_id)
	SELECT * FROM proposal_topographies			(NOLOCK) WHERE proposal_id=@proposal_id
	SELECT * FROM proposal_detail_worksheets	(NOLOCK) WHERE proposal_detail_id IN (SELECT id FROM proposal_details (NOLOCK) WHERE proposal_id=@proposal_id)
	SELECT * FROM proposal_original_flights		(NOLOCK) WHERE proposal_id=@proposal_id
	SELECT * FROM proposal_sales_models			(NOLOCK) WHERE proposal_id=@proposal_id

	--SELECT * FROM proposals (NOLOCK) WHERE id=@proposal_id
	--SELECT * FROM proposal_flights (NOLOCK) WHERE proposal_id=@proposal_id
	--SELECT * FROM proposal_audiences (NOLOCK) WHERE proposal_id=@proposal_id 
	--SELECT * FROM proposal_employees (NOLOCK) WHERE proposal_id=@proposal_id
	--SELECT * FROM proposal_contacts (NOLOCK) WHERE proposal_id=@proposal_id
	--SELECT * FROM proposal_details (NOLOCK) WHERE proposal_id=@proposal_id
	--SELECT * FROM proposal_detail_audiences (NOLOCK) WHERE proposal_detail_id IN (SELECT id FROM proposal_details (NOLOCK) WHERE proposal_id=@proposal_id)
	--SELECT * FROM proposal_topographies (NOLOCK) WHERE proposal_id=@proposal_id
END



