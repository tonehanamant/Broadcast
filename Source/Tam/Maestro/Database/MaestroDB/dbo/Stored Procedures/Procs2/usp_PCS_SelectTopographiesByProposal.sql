-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_SelectTopographiesByProposal]
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
		topography_type
	FROM
		topographies (NOLOCK) 
	WHERE 
		id IN (SELECT topography_id FROM proposal_topographies (NOLOCK) WHERE proposal_id=@proposal_id)
END
