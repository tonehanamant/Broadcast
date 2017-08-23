
-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_ACS_AutoValidation_UpdateInvoiceSystemId]
	@invoice_id INT,
	@media_month_id INT
AS
BEGIN
	DECLARE @zone_id INT
	DECLARE @air_date DATETIME

	SELECT TOP 1
		@zone_id=zone_id,
		@air_date=air_date
	FROM
		affidavits (NOLOCK)
	WHERE
		media_month_id=@media_month_id
		AND invoice_id=@invoice_id 
		AND zone_id IS NOT NULL
		AND air_date IS NOT NULL

	IF @zone_id IS NOT NULL AND @air_date IS NOT NULL
		BEGIN			
			UPDATE 
				invoices 
			SET 
				system_id=(
					SELECT TOP 1
						system_id
					FROM
						uvw_systemzone_universe (NOLOCK) 
					WHERE 
						type='BILLING'
						AND (start_date<=@air_date AND (end_date>=@air_date OR end_date IS NULL))
						AND zone_id=@zone_id
					) 
			WHERE 
				id=@invoice_id
				AND system_id IS NULL
				
			UPDATE 
				invoices 
			SET 
				invoicing_system_id=(
					SELECT TOP 1
						system_id
					FROM
						uvw_systemzone_universe (NOLOCK) 
					WHERE 
						type='INVOICING'
						AND (start_date<=@air_date AND (end_date>=@air_date OR end_date IS NULL))
						AND zone_id=@zone_id
					) 
			WHERE 
				id=@invoice_id
				AND invoicing_system_id IS NULL
		END
END

