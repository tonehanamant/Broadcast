CREATE TYPE [dbo].[CustomRatingsRequest] AS TABLE (
    [id]          INT NOT NULL,
    [network_id]  INT NOT NULL,
    [audience_id] INT NOT NULL,
    PRIMARY KEY CLUSTERED ([id] ASC));

