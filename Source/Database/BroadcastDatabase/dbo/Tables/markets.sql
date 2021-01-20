CREATE TABLE [dbo].[markets] (
    [market_code]    SMALLINT     NOT NULL,
    [geography_name] VARCHAR (31) NOT NULL,
    CONSTRAINT [PK_markets] PRIMARY KEY CLUSTERED ([market_code] ASC)
);

