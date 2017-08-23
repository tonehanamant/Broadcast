CREATE TABLE [dbo].[zone_zip_code] (
    [zone_id]        INT      NOT NULL,
    [zip_code]       CHAR (5) NOT NULL,
    [effective_date] DATETIME NULL,
    FOREIGN KEY ([zone_id]) REFERENCES [dbo].[zones] ([id])
);

