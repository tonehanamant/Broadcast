
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].usp_PCS_GetOrderedPostingFlights
	@proposal_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	DECLARE @original_proposal_id INT;
	SET @original_proposal_id = (SELECT original_proposal_id FROM proposals (NOLOCK) WHERE id=@proposal_id);

    SELECT 
		pf.start_date,
		pf.end_date,
		pf.selected,
		mm.month,
		mw.week_number
	FROM 
		proposal_flights pf		(NOLOCK) 
		JOIN media_weeks mw		(NOLOCK) ON pf.start_date BETWEEN mw.start_date AND mw.end_date
		JOIN media_months mm	(NOLOCK) ON mw.media_month_id=mm.id
	WHERE 
		proposal_id=@original_proposal_id
END