


-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_ARS_GetNielsenNetworks]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT 
		id,
		network_rating_category_id,
		nielsen_id,
		code,
		name,
		active,
		effective_date
	FROM 
		nielsen_networks 
	ORDER BY
		code
END



