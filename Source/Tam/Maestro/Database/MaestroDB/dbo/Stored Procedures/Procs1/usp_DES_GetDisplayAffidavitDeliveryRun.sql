-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 3/20/2013
-- Description:	
-- =============================================
CREATE PROCEDURE usp_DES_GetDisplayAffidavitDeliveryRun
	@affidavit_delivery_run_id INT
AS
BEGIN
	SET NOCOUNT ON;

	-- affidavit_delivery_runs
    SELECT
		adr.*
	FROM
		dbo.affidavit_delivery_runs adr (NOLOCK)
	WHERE
		adr.id=@affidavit_delivery_run_id
		
	-- affidavit_delivery_run_messages
    SELECT
		adrm.*
	FROM
		dbo.affidavit_delivery_run_messages adrm (NOLOCK)
	WHERE
		adrm.affidavit_delivery_run_id=@affidavit_delivery_run_id
		
	-- employees
    SELECT
		e.*
	FROM
		dbo.affidavit_delivery_runs adr (NOLOCK)
		JOIN dbo.employees e (NOLOCK) ON e.id=adr.started_by_employee_id
	WHERE
		adr.id=@affidavit_delivery_run_id
		
	-- media_months
    SELECT
		mm.*
	FROM
		dbo.affidavit_delivery_runs adr (NOLOCK)
		JOIN dbo.media_months mm (NOLOCK) ON mm.id=adr.media_month_id
	WHERE
		adr.id=@affidavit_delivery_run_id
END
