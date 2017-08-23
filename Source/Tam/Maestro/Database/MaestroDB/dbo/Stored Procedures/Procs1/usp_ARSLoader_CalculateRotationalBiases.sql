-- =============================================
-- Author:		Mike Deaven
-- Create date: 6/10/2014
-- Description:	Runs the stored procedures in the Rotational Bias Calculations
-- =============================================
CREATE PROCEDURE [dbo].[usp_ARSLoader_CalculateRotationalBiases]
	@baseMediaMonth varchar(15),
	@mediaMonth varchar(15),
	@ratingCategoryId int
AS
BEGIN
	EXEC dbo.usp_ARSLoader_DeleteRotationalBiases @baseMediaMonth, @ratingCategoryId;
	EXEC dbo.usp_ARSLoader_CalculateRotationalBiases3Month @baseMediaMonth, @mediaMonth, @ratingCategoryId;
	EXEC dbo.usp_ARSLoader_CalculateRotationalBiases4Year @baseMediaMonth, @mediaMonth, @ratingCategoryId;
END
