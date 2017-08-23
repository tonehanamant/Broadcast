-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 5/6/2011
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_ACS_GetBusinesses]
	@media_month_id INT
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

	CREATE TABLE #businesses (id INT, name VARCHAR(MAX))
	INSERT INTO #businesses
		SELECT DISTINCT
			b.id,
			b.name
		FROM
			#systems s
			JOIN businesses b (NOLOCK) ON b.id=dbo.GetBusinessIdFromSystemId(s.id,@effective_date)

	SELECT 
		businesses.* 
	FROM 
		businesses (NOLOCK)
		JOIN #businesses b ON b.id=businesses.id
	ORDER BY
		businesses.name

	DROP TABLE #systems;
	DROP TABLE #businesses;
END
