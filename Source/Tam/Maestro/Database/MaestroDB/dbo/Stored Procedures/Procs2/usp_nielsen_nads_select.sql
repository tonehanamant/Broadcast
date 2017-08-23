-- =============================================
-- Author:		CRUD Creator
-- Create date: 11/07/2014 09:10:36 AM
-- Description:	Auto-generated method to delete or potentionally disable a nielsen_nads record.
-- =============================================
CREATE PROCEDURE [dbo].[usp_nielsen_nads_select]
	@start_date DATETIME,
	@end_date DATETIME,
	@market_break VARCHAR(63),
	@audience_id INT,
	@network_id INT,
	@daypart_id INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[nielsen_nads].*
	FROM
		[dbo].[nielsen_nads] WITH(NOLOCK)
	WHERE
		[start_date]=@start_date
		AND [end_date]=@end_date
		AND [market_break]=@market_break
		AND [audience_id]=@audience_id
		AND [network_id]=@network_id
		AND [daypart_id]=@daypart_id
END
