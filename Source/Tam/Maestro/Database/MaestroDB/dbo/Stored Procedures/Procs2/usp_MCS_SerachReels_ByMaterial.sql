

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_MCS_SerachReels_ByMaterial]
	@copy VARCHAR(31)
AS
BEGIN
	DECLARE @material_id INT
	SELECT @material_id = m.id FROM materials m (NOLOCK) WHERE m.code=@copy;
	
    EXEC usp_MCS_SerachReels_ByMaterialId @material_id
END

