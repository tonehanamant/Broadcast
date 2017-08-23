CREATE PROCEDURE [dbo].[usp_RCS_GetTrafficRateCardItem] 
@rate_card_id int

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT 
		id,
		convert(varchar, start_date, 110) + ' - ' +
		CASE
			WHEN 
				end_date is NULL
			THEN 
				'Present'
			ELSE
				convert(varchar, end_date, 110)
		END AS end_date
			
	FROM
		traffic_rate_cards (nolock)
	WHERE
		id = @rate_card_id
END
