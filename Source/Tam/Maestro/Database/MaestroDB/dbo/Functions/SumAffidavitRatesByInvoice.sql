-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION dbo.SumAffidavitRatesByInvoice
(
	@invoice_id INT,
	@valid_only BIT
)
RETURNS MONEY
AS
BEGIN
	DECLARE @return MONEY

	SET @return = (
		SELECT 
			CAST(SUM(a.rate)/100.0 AS MONEY) 
		FROM 
			affidavits a (NOLOCK)
		WHERE
			a.invoice_id=@invoice_id
			AND (@valid_only = 0 OR a.status_id=1)
	)

	RETURN @return;
END
