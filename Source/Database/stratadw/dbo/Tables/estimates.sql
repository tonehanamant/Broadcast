CREATE TABLE [dbo].[estimates] (
    [estimate_id]       BIGINT         NOT NULL,
    [flight_start_date] DATE           NOT NULL,
    [flight_end_date]   DATE           NOT NULL,
    [description]       VARCHAR (4096) NOT NULL,
    [estimate_group]    VARCHAR (1024) NOT NULL,
    [product_name]      VARCHAR (256)  NOT NULL,
    [estimate_code]     VARCHAR(64) NOT NULL DEFAULT '', 
    [plan_id]           INT NULL, 
    CONSTRAINT [PK_estimates] PRIMARY KEY CLUSTERED ([estimate_id] ASC)
);

