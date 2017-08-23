
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectDisplayTopographies]
	@effective_date DATETIME
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT	
		id,
		code,
		name,
		topography_type,
		dbo.GetSubscribersForTopographyByDate(id, @effective_date)
	FROM 
		topographies (NOLOCK)
	WHERE	
		code<>'CUS'
	ORDER BY 
		name
END

