CREATE PROCEDURE [dbo].[usp_tam_post_analysis_reports_iscis_delete]
(
	@tam_post_proposal_id		Int,
	@audience_id		Int,
	@network_id		Int,
	@material_id		Int,
	@isci_material_id		Int,
	@enabled		Bit)
AS
DELETE FROM
	tam_post_analysis_reports_iscis
WHERE
	tam_post_proposal_id = @tam_post_proposal_id
 AND
	audience_id = @audience_id
 AND
	network_id = @network_id
 AND
	material_id = @material_id
 AND
	isci_material_id = @isci_material_id
 AND
	enabled = @enabled
