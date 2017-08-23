-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectNetworkItemsByDate]
	@active bit,
	@effective_date datetime
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT 
		network_id,
		code,
		start_date 
	FROM 
		uvw_network_universe (NOLOCK) 
	WHERE 
		((@effective_date IS NULL AND end_date IS NULL) OR (start_date<=@effective_date AND (end_date>=@effective_date OR end_date IS NULL)))
		AND (@active IS NULL OR active=@active)
	ORDER BY 
		code
END
