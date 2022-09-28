CREATE TABLE [dbo].[estimate_order_details] (
    [estimate_id]              BIGINT        NOT NULL,
    [source_market_name]       VARCHAR (128) NOT NULL,
    [market_code]              SMALLINT      NULL,
    [order_date]               DATE          NOT NULL,
    [order_start_date]         DATE          NOT NULL,
    [order_end_date]           DATE          NOT NULL,
    [days_of_week]             VARCHAR (64)  NOT NULL,
    [order_start_time_seconds] INT           NOT NULL,
    [order_end_time_seconds]   INT           NOT NULL,
    [affiliation]              VARCHAR (64)  NOT NULL,
    [station_call_letters]     VARCHAR (64)  NOT NULL,
    [survey_name]              VARCHAR (256) NOT NULL,
    [vendor_name]              VARCHAR (128) NOT NULL,
    [program_name]             VARCHAR (256) NOT NULL,
    [source_audience]          VARCHAR (128) NOT NULL,
    [audience_id]              INT           NULL,
    [audience_ordinal]         SMALLINT      NOT NULL,
    [impressions]              FLOAT (53)    NOT NULL,
    [rating]                   FLOAT (53)    NULL
);

