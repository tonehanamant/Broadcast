-- =============================================
-- Author:		Stephen Defusco
-- Create date: 08/28/2014
-- Description:	Gets the Quaters and Years from Posted items
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetPostedQuarterAndYears]
AS
BEGIN
       SET NOCOUNT ON;
       
       SELECT 
              CASE mm.month WHEN 1 THEN 1 WHEN 2 THEN 1 WHEN 3 THEN 1 WHEN 4 THEN 2 WHEN 5 THEN 2 WHEN 6 THEN 2 WHEN 7 THEN 3 WHEN 8 THEN 3 WHEN 9 THEN 3 WHEN 10 THEN 4 WHEN 11 THEN 4 WHEN 12 THEN 4 END quarter,
              mm.year,
              MIN(mm.start_date) quarter_start_date,
              MAX(mm.end_date) quarter_end_date
       FROM 
              media_months mm (NOLOCK)
       WHERE
              mm.id IN (
                     SELECT DISTINCT
                           p.posting_media_month_id
                     FROM
                           tam_post_proposals tpp (NOLOCK)
                           JOIN proposals p (NOLOCK) ON p.id=tpp.posting_plan_proposal_id
                           JOIN tam_posts tp (NOLOCK) ON tp.id=tpp.tam_post_id
                                  AND tp.is_deleted=0        -- posts that haven't been market deleted
                                  AND tp.post_type_code=1 -- posts that have been marked "Official"
              )
       GROUP BY
              CASE mm.month WHEN 1 THEN 1 WHEN 2 THEN 1 WHEN 3 THEN 1 WHEN 4 THEN 2 WHEN 5 THEN 2 WHEN 6 THEN 2 WHEN 7 THEN 3 WHEN 8 THEN 3 WHEN 9 THEN 3 WHEN 10 THEN 4 WHEN 11 THEN 4 WHEN 12 THEN 4 END,
              mm.year
       ORDER BY
              mm.year DESC,
              MIN(mm.start_date)
END
