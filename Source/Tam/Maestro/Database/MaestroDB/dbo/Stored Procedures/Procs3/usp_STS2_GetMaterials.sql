	-- =============================================
	-- Author:		Stephen DeFusco
	-- Create date: 8/12/2010
	-- Description:	<Description,,>
	-- =============================================
	CREATE PROCEDURE [dbo].[usp_STS2_GetMaterials]
	AS
	BEGIN
		-- SET NOCOUNT ON added to prevent extra result sets from
		-- interfering with SELECT statements.
		SET NOCOUNT ON;

		SELECT DISTINCT
			m.*
		FROM
			materials m (NOLOCK)
		WHERE
			active=1
	END
