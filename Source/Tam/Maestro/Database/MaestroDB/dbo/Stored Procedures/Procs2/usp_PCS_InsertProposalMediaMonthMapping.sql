CREATE PROCEDURE [dbo].[usp_PCS_InsertProposalMediaMonthMapping]
	@proposal_id INT,
	@media_month_id INT,
	@spot_length_id INT
AS
BEGIN
	DELETE FROM proposal_media_month_marriage_mappings 
	WHERE 
		proposal_id = @proposal_id
		and media_month_id = @media_month_id
		and spot_length_id = @spot_length_id;
	
	INSERT INTO proposal_media_month_marriage_mappings(proposal_id, media_month_id, spot_length_id)
	VALUES (@proposal_id, @media_month_id, @spot_length_id);
END
