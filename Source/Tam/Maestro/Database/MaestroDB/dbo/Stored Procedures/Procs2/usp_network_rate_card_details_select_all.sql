﻿CREATE PROCEDURE usp_network_rate_card_details_select_all
AS
BEGIN
SELECT
	*
FROM
	dbo.network_rate_card_details WITH(NOLOCK)
END
