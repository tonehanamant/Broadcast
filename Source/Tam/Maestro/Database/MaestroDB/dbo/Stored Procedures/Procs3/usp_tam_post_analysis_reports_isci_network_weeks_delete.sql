CREATE PROCEDURE [dbo].[usp_tam_post_analysis_reports_isci_network_weeks_delete]
(
	@tam_post_proposal_id		Int,
	@audience_id		Int,
	@network_id		Int,
	@media_week_id		Int,
	@material_id		Int,
	@enabled		Bit)
AS
DELETE FROM
	tam_post_analysis_reports_isci_network_weeks
WHERE
	tam_post_proposal_id = @tam_post_proposal_id
 AND
	audience_id = @audience_id
 AND
	network_id = @network_id
 AND
	media_week_id = @media_week_id
 AND
	material_id = @material_id
 AND
	enabled = @enabled
