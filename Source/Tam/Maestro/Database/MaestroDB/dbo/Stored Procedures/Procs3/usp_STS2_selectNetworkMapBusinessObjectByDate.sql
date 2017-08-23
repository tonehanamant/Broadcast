-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectNetworkMapBusinessObjectByDate]
	@network_id INT,
	@map_set VARCHAR(15),
	@effective_date DATETIME
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT
		nm.network_map_id,
		nm.start_date,
		nm.network_id,
		nm.map_set,
		nm.map_value,
		nm.active,
		nm.flag,
		nm.end_date
	FROM
		uvw_networkmap_universe nm
	WHERE
		(nm.start_date<=@effective_date AND (nm.end_date>=@effective_date OR nm.end_date IS NULL))
		AND nm.network_id=@network_id
		AND nm.map_set=@map_set
END
