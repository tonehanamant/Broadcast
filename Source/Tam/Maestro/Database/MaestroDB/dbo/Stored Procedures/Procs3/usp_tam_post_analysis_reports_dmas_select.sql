CREATE PROCEDURE usp_tam_post_analysis_reports_dmas_select
(
	@tam_post_proposal_id		Int,
	@audience_id		Int,
	@enabled		Bit
)
AS
SELECT
	*
FROM
	tam_post_analysis_reports_dmas WITH(NOLOCK)
WHERE
	tam_post_proposal_id=@tam_post_proposal_id
	AND
	audience_id=@audience_id
	AND
	enabled=@enabled

