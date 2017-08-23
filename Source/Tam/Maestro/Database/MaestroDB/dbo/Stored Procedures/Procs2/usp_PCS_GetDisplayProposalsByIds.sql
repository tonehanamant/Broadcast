CREATE PROCEDURE [dbo].[usp_PCS_GetDisplayProposalsByIds]
	@ids VARCHAR(MAX)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT 
		dp.*
	FROM 
		uvw_display_proposals dp
	WHERE
		dp.id IN (
			SELECT id FROM dbo.SplitIntegers(@ids)
		)
	ORDER BY 
		dp.id DESC
END
