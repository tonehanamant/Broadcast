-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetDisplayDaypartForProposal]
	@proposal_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT 
		id,
		code,
		name,
		start_time,
		end_time,
		mon,
		tue,
		wed,
		thu,
		fri,
		sat,
		sun 
	FROM 
		vw_ccc_daypart (NOLOCK) 
	WHERE 
		id=(
			SELECT default_daypart_id FROM proposals (NOLOCK) WHERE id=@proposal_id
		)
END
