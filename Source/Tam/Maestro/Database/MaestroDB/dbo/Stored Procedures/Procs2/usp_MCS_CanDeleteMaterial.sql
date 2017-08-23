
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_MCS_CanDeleteMaterial]
	@material_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    DECLARE @return AS INT

	SET @return = 0

	SET @return = @return + (
		SELECT COUNT(*) FROM affidavits WITH (NOLOCK) WHERE material_id=@material_id
	)
	SET @return = @return + (
		SELECT COUNT(*) FROM reel_materials WITH (NOLOCK) WHERE material_id=@material_id
	)
	SET @return = @return + (
		SELECT COUNT(*) FROM material_revisions WITH (NOLOCK) WHERE original_material_id=@material_id
	)

	SELECT @return
END

