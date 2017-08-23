CREATE TABLE [dbo].[zone_zip_codes] (
    [zone_id]        INT      NOT NULL,
    [zip_code]       CHAR (5) NOT NULL,
    [effective_date] DATETIME NOT NULL,
    CONSTRAINT [PK_zone_zip_codes] PRIMARY KEY CLUSTERED ([zone_id] ASC, [zip_code] ASC),
    FOREIGN KEY ([zone_id]) REFERENCES [dbo].[zones] ([id])
);

