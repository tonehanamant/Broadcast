-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 11/11/2015
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetNetworksByDate]
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
		n.start_date<=@effective_date AND (n.end_date>=@effective_date OR n.end_date IS NULL)
END