-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 3/13/2013
-- Description:	Deletes previous year of nielsen county data after a new matching year file is loaded.
-- =============================================
CREATE PROCEDURE usp_ARS_DeleteNielsenCountyUniverseEstimateData
	@new_document_id INT,
	@effective_date DATE
AS
BEGIN
	DELETE FROM 
		dbo.nielsen_county_universe_estimates
	WHERE 
		effective_date=@effective_date 
		AND document_id<>@new_document_id
END
