CREATE TABLE [dbo].[zone_zip_code_histories] (
    [zone_id]    INT      NOT NULL,
    [zip_code]   CHAR (5) NOT NULL,
    [start_date] DATETIME NOT NULL,
    [end_date]   DATETIME NOT NULL,
    CONSTRAINT [PK_zone_zip_code_histories] PRIMARY KEY CLUSTERED ([zone_id] ASC, [zip_code] ASC, [start_date] ASC),
    FOREIGN KEY ([zone_id]) REFERENCES [dbo].[zones] ([id])
);

