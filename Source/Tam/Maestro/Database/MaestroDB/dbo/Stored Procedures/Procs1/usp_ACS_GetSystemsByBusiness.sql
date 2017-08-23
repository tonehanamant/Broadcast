
/***************************************************************************************************
** Date         Author          Description   
** ---------    ----------		-----------------------------------------------------
** 5/6/2011		Stephen DeFusco
** 03/13/2017	Abdul Sukkur	Added new columns to systems 
*****************************************************************************************************/
-- usp_ACS_GetSystemsByBusiness 354,8
CREATE PROCEDURE [dbo].[usp_ACS_GetSystemsByBusiness]
	@media_month_id INT,
	@business_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @effective_date DATETIME
	SELECT @effective_date = mm.start_date FROM media_months mm (NOLOCK) WHERE mm.id=@media_month_id

    CREATE TABLE #systems (id INT)
	INSERT INTO #systems
		SELECT DISTINCT 
			i.system_id 
		FROM 
			invoices i (NOLOCK) 
		WHERE 
			i.media_month_id=@media_month_id
			AND i.system_id IS NOT NULL 

	SELECT 
		systems.* 
	FROM 
		systems (NOLOCK)
		JOIN #systems s ON s.id=systems.id
	WHERE
		(@business_id IS NULL OR dbo.GetBusinessIdFromSystemId(systems.id,@effective_date)=@business_id)
	ORDER BY
		systems.code

	DROP TABLE #systems;
END
