-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_MCS_GetMaterialMapHistories_ForMaterial]
	@material_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT
		mmh.*
	FROM 
		material_map_histories mmh (NOLOCK)
	WHERE
		mmh.material_id=@material_id
END
