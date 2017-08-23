
CREATE PROCEDURE [dbo].[usp_ARS_GetNetworkBreaks]
	@nielsen_network_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT
		id,
		nielsen_network_id,
		seconds_after_hour,
		length,
		effective_date,
		active
	FROM
		network_breaks WITH(NOLOCK)
	WHERE
		nielsen_network_id=@nielsen_network_id
	AND active = 1
END
