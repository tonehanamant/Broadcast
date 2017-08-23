CREATE AGGREGATE [dbo].[Agg_Median](@value FLOAT (53))
    RETURNS FLOAT (53)
    EXTERNAL NAME [SqlTest].[SqlTest.Agg_Median];

