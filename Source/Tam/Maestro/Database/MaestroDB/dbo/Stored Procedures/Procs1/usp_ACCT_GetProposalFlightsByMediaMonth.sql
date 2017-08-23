-- =============================================
-- Author:        John Carsley
-- Create date: 10/24/2011
-- Description:   Gets only the flight weeks inside a media month
-- Usage : exec usp_ACCT_GetProposalFlightsByMediaMonth 36063, 363
-- =============================================
CREATE PROCEDURE [dbo].[usp_ACCT_GetProposalFlightsByMediaMonth]
      @proposal_id int
      ,@media_month_id int
AS 
BEGIN
      SET NOCOUNT ON;

      SELECT pf.[proposal_id]
              ,pf.[start_date]
              ,pf.[end_date]
              ,pf.[selected]
      FROM proposal_flights pf WITH (NOLOCK)
      WHERE proposal_id = @proposal_id
      and exists (
            SELECT 1
            FROM media_weeks mw  WITH (NOLOCK)
                  inner join media_months mm  WITH (NOLOCK) on mm.id =  mw.media_month_id
            WHERE pf.start_date between mw.start_date and mw.end_date
            and pf.end_date between mw.start_date and mw.end_date
            and media_month_id = @media_month_id)
END
