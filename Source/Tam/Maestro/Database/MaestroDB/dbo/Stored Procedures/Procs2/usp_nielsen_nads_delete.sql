-- =============================================
-- Author:		CRUD Creator
-- Create date: 11/07/2014 09:10:36 AM
-- Description:	Auto-generated method to delete a single nielsen_nads record.
-- =============================================
CREATE PROCEDURE [dbo].[usp_nielsen_nads_delete]
	@start_date DATETIME,
	@end_date DATETIME,
	@market_break VARCHAR(63),
	@audience_id INT,
	@network_id INT,
	@daypart_id INT
AS
BEGIN
	DELETE FROM
		[dbo].[nielsen_nads]
	WHERE
		[start_date]=@start_date
		AND [end_date]=@end_date
		AND [market_break]=@market_break
		AND [audience_id]=@audience_id
		AND [network_id]=@network_id
		AND [daypart_id]=@daypart_id
END
