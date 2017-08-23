-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[GetTotalAffidavitsWithTrafficId]
(
	@traffic_id INT,
	@system_id INT,
	@media_month_id INT
)
RETURNS FLOAT
AS
BEGIN
	DECLARE @return FLOAT

	SET @return = (
		SELECT
			CAST(COUNT(a.id) AS FLOAT)
		FROM
			affidavits a	(NOLOCK)
			JOIN invoices i	(NOLOCK) ON i.id=a.invoice_id
		WHERE
			a.media_month_id=@media_month_id
			AND i.system_id=@system_id
			and a.traffic_id=@traffic_id
	)

	RETURN @return
END
