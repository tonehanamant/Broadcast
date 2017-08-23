CREATE PROCEDURE [dbo].[usp_PCS_GetMarriedProposalIDsForMediaMonth]
	@media_month_id INT,
	@spot_length_id INT
AS
BEGIN
	SELECT proposal_id from proposal_media_month_marriage_mappings WITH (NOLOCK)
	WHERE	
		media_month_id = @media_month_id and spot_length_id = @spot_length_id;
END
