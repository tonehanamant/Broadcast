
-- =============================================
-- Author:		CRUD Creator
-- Create date: 01/30/2017 12:09:59 PM
-- Description:	Auto-generated method to update a proposal_marry_candidates record.
-- =============================================
CREATE PROCEDURE usp_proposal_marry_candidates_update
	@id INT,
	@proposal_id INT,
	@media_month_id INT,
	@spot_length_id INT,
	@base_index DECIMAL(19,2)
AS
BEGIN
	UPDATE
		[dbo].[proposal_marry_candidates]
	SET
		[proposal_id]=@proposal_id,
		[media_month_id]=@media_month_id,
		[spot_length_id]=@spot_length_id,
		[base_index]=@base_index
	WHERE
		[id]=@id
END

