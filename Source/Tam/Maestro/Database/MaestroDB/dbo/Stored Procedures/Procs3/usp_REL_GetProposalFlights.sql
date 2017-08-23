
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_REL_GetProposalFlights]
	@proposal_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT
		proposal_id,
		start_date,
		end_date,
		selected 
	FROM
		proposal_flights (NOLOCK)
	WHERE
		proposal_id=@proposal_id
END

