CREATE TYPE [dbo].[AudienceInformation] AS TABLE (
    [rating_audience_id] INT NOT NULL,
    [custom_audience_id] INT NOT NULL,
    PRIMARY KEY CLUSTERED ([rating_audience_id] ASC, [custom_audience_id] ASC));

