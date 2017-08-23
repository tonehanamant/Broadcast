-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[GetAudienceSortPositionFromID] 
(
	@audienceID INT
)
RETURNS INT
AS
BEGIN
	DECLARE 
		@typeSortPosition as int,
		@startAgeSortPosition as int,
		@endAgeSortPosition as int;

	select
		@typeSortPosition = 
		case a.sub_category_code
			when 'H' then 10000
			when 'F' then 30000
			when 'W' then 30000
			when 'M' then 40000
			when 'P' then 50000
			when 'K' then 60000
			when 'T' then 70000
			when 'A' then 80000
		end,
		@startAgeSortPosition = a.range_start * 100,
		@endAgeSortPosition = a.range_end
	from
		audiences a (NOLOCK)
	where
		a.id = @audienceID;
		
	RETURN @typeSortPosition + @startAgeSortPosition + @endAgeSortPosition;
END
