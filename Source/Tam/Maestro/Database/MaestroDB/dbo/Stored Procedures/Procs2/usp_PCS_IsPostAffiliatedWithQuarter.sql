-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 7/25/2011
-- Description:	
-- =============================================
-- usp_PCS_IsPostAffiliatedWithQuarter 100001,2011,2
CREATE PROCEDURE [dbo].[usp_PCS_IsPostAffiliatedWithQuarter]
	@tam_post_id INT,
	@year INT,
	@quarter INT
AS
BEGIN
	DECLARE @num_records INT
	
	SELECT
		@num_records = COUNT(*)
	FROM
		tam_post_proposals tpp (NOLOCK)
		JOIN proposals p (NOLOCK) ON p.id = tpp.posting_plan_proposal_id
		JOIN media_months mm (NOLOCK) ON mm.id=p.posting_media_month_id
	WHERE
		tpp.tam_post_id=@tam_post_id
		AND mm.year = @year
		AND CASE mm.month WHEN 1 THEN 1 WHEN 2 THEN 1 WHEN 3 THEN 1 WHEN 4 THEN 2 WHEN 5 THEN 2 WHEN 6 THEN 2 WHEN 7 THEN 3 WHEN 8 THEN 3 WHEN 9 THEN 3 WHEN 10 THEN 4 WHEN 11 THEN 4 WHEN 12 THEN 4 END = @quarter

		
	IF @num_records > 0
		SELECT CAST(1 AS BIT)
	ELSE
		SELECT CAST(0 AS BIT)
END
