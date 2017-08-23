
CREATE PROCEDURE [dbo].[usp_FIN_GetMediaMonths_Names]

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
SET NOCOUNT ON;
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED

SELECT     
media_month, CAST(DATENAME(month, CAST(CAST(month AS varchar) + '/01/' + CAST(year AS varchar) AS datetime)) AS varchar) 
                      + ', ' + CAST(year AS varchar) AS MonthName
FROM         media_months AS mm  
WHERE year = year(getdate()) Or --Current Year
year = year(getdate()) - 1 --Prev Year
Or start_date Between getdate() And DateAdd("d",90,getdate()) --Three Additional FUTURE Media Months
ORDER BY year DESC, month DESC

End