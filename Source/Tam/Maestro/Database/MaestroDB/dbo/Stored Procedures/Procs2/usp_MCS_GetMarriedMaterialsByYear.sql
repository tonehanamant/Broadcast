-- =============================================
-- Author:      <Author,,Name>
-- Create date: <Create Date,,>
-- Description: <Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_MCS_GetMarriedMaterialsByYear]
	@year INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT 
		materials.*
	FROM 
		materials (NOLOCK)
	WHERE
		materials.type='Married'
		AND (year(date_received)=@year OR year(date_created)=@year)
	ORDER BY 
		code
END
