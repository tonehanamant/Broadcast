-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetNetworkByDate]
	@network_id INT,
	@effective_date DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		network_id,
		code,
		[name],
		active,
		flag,
		start_date,
		language_id, 
		affiliated_network_id, 
		network_type_id
	FROM
		uvw_network_universe n (NOLOCK)
	WHERE
		n.network_id=@network_id
		AND n.start_date<=@effective_date AND (n.end_date>=@effective_date OR n.end_date IS NULL)
END

