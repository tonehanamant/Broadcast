﻿-- =============================================
-- Author:		CRUD Creator
-- Create date: 05/01/2014 11:23:33 AM
-- Description:	Auto-generated method to delete or potentionally disable a universes record.
-- =============================================
CREATE PROCEDURE [dbo].[usp_universes_select]
	@rating_category_id INT,
	@base_media_month_id INT,
	@forecast_media_month_id INT,
	@nielsen_network_id INT,
	@audience_id INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[universes].*
	FROM
		[dbo].[universes] WITH(NOLOCK)
	WHERE
		[rating_category_id]=@rating_category_id
		AND [base_media_month_id]=@base_media_month_id
		AND [forecast_media_month_id]=@forecast_media_month_id
		AND [nielsen_network_id]=@nielsen_network_id
		AND [audience_id]=@audience_id
END
