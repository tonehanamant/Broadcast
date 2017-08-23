CREATE TYPE [dbo].[MediaWeeksInput] AS TABLE (
    [media_month_id] INT NOT NULL,
    [week_number]    INT NOT NULL,
    [selected]       BIT NOT NULL,
    PRIMARY KEY CLUSTERED ([media_month_id] ASC, [week_number] ASC));

