
		-- =============================================
	-- Author:		<Nicholas Kheynis>
	-- Create date: <3/26/15>
	-- Description:	<Get Primary Daypart from Proposal,,>
	-- =============================================
	CREATE PROCEDURE [dbo].[usp_PCS_GetPrimaryDisplayDaypartForProposal]
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
				SELECT primary_daypart_id FROM proposals (NOLOCK) WHERE id=@proposal_id
			)
	END
