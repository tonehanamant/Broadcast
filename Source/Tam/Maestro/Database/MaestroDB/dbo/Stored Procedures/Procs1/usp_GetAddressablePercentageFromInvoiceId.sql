	-- =============================================
	-- Author:		Stephen DeFusco
	-- Create date: 
	-- Description:	<Description,,>
	-- =============================================
	CREATE PROCEDURE [dbo].[usp_GetAddressablePercentageFromInvoiceId] 
		@invoice_id INT
	AS
	BEGIN
		SELECT 
			af.addressable_percentage 
		FROM
			dbo.invoices i (NOLOCK) 
			JOIN dbo.affidavit_files af (NOLOCK) ON af.id=i.affidavit_file_id
		WHERE
			i.id=@invoice_id
	END
