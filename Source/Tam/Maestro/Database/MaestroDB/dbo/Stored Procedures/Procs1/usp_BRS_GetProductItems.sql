

CREATE PROCEDURE [dbo].[usp_BRS_GetProductItems]
@advertiserId int

AS
BEGIN

	SET NOCOUNT ON;

	SELECT id,
			[name]
	FROM
		cmw_traffic_products (nolock)
	WHERE
		cmw_traffic_advertisers_id = @advertiserId
	ORDER BY [name] ASC
END

