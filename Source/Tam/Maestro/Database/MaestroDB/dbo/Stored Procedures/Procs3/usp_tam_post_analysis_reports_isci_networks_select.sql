CREATE PROCEDURE usp_tam_post_analysis_reports_isci_networks_select
(
	@tam_post_proposal_id		Int,
	@audience_id		Int,
	@network_id		Int,
	@material_id		Int,
	@enabled		Bit
)
AS
SELECT
	*
FROM
	tam_post_analysis_reports_isci_networks WITH(NOLOCK)
WHERE
	tam_post_proposal_id=@tam_post_proposal_id
	AND
	audience_id=@audience_id
	AND
	network_id=@network_id
	AND
	material_id=@material_id
	AND
	enabled=@enabled

