CREATE TABLE [dbo].[agency_code_mapset] (
    [agency_code] VARCHAR (63)  NOT NULL,
    [map_set]     VARCHAR (63)  NOT NULL,
    [map_value]   VARCHAR (255) NOT NULL,
    CONSTRAINT [PK_agency_code_mapset] PRIMARY KEY CLUSTERED ([agency_code] ASC, [map_set] ASC)
);

