
CREATE PROCEDURE [dbo].[usp_PCS_GetMediaMonthsForDailyImsForecast]
AS
BEGIN
       SET NOCOUNT ON;
       SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

       DECLARE @start_date DATE;
       SELECT
              @start_date = mm.start_date
       FROM
              media_months mm
       WHERE
              CONVERT(DATE,GETDATE()) BETWEEN mm.start_date AND mm.end_date
              
       DECLARE @media_month_ids TABLE (id INT);
       INSERT INTO @media_month_ids
              SELECT DISTINCT
                     mw.media_month_id
              FROM
                     proposals p
                     JOIN proposal_details pd ON pd.proposal_id=p.id
                     JOIN proposal_detail_worksheets pdw ON pdw.proposal_detail_id=pd.id
                           AND pdw.units>0
                     JOIN media_weeks mw ON mw.id=pdw.media_week_id
                     JOIN media_months mm ON mm.id=mw.media_month_id
              WHERE
                     p.include_on_availability_planner=1
                     AND p.total_gross_cost>0 -- filter out zero cost plans
                     AND p.start_date>=@start_date 

       SELECT
              mm.*
       FROM
              media_months mm
              JOIN @media_month_ids mmi ON mmi.id=mm.id
       ORDER BY
              mm.start_date
END