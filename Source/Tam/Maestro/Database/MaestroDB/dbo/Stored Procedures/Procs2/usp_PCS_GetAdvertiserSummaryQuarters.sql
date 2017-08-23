CREATE PROCEDURE [dbo].[usp_PCS_GetAdvertiserSummaryQuarters]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT DISTINCT
		CASE mm.month 
			WHEN 1 THEN 1 
			WHEN 2 THEN 1 
			WHEN 3 THEN 1 
			WHEN 4 THEN 2 
			WHEN 5 THEN 2 
			WHEN 6 THEN 2 
			WHEN 7 THEN 3 
			WHEN 8 THEN 3 
			WHEN 9 THEN 3 
			WHEN 10 THEN 4 
			WHEN 11 THEN 4 
			WHEN 12 THEN 4 
		END AS 'quarter',
		mm.year
	FROM tam_posts tp							(NOLOCK)
		INNER JOIN tam_post_proposals tpp		(NOLOCK) ON tpp.tam_post_id=tp.id
		INNER JOIN proposals p					(NOLOCK) ON p.id=tpp.posting_plan_proposal_id
		INNER JOIN media_months mm				(NOLOCK) ON mm.id=p.posting_media_month_id
	WHERE  tp.is_deleted=0			-- posts that haven't been market deleted
		AND tp.post_type_code=1 -- posts that have been marked "Official"
	ORDER BY mm.year, quarter
END
