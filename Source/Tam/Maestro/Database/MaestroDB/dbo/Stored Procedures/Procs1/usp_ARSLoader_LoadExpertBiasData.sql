-- =============================================
-- Author:		Mike Deaven
-- Create date: 9/28/2012
-- Description:	Loads a row of data into the expert bias temp table
-- =============================================
CREATE PROCEDURE usp_ARSLoader_LoadExpertBiasData
	@network VARCHAR(15),
	@dmaCode INT,
	@universe FLOAT,
	@rawHH FLOAT,
	@rawRtg FLOAT,
	@rotationalBias FLOAT,
	@expertBias FLOAT,
	@finalHH FLOAT,
	@rating FLOAT
AS
BEGIN
	INSERT INTO temp_db_backup.dbo.expert_bias_ratings_adjustments
	VALUES (@network, @dmaCode, @universe, @rawHH, @rawRtg, @rotationalBias, @expertBias, @finalHH, @rating);
END
