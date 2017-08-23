

 

-- =============================================

-- Author:        <Author,,Name>

-- Create date: <Create Date,,>

-- Description:   <Description,,>

-- =============================================

CREATE PROCEDURE [dbo].[usp_RCS_GetAllRateCardNetworks]

AS

BEGIN

 

select distinct network_id from rate_card_details (NOLOCK) where rate_card_id in

(select id from rate_cards (NOLOCK) where rate_card_book_id=(select max(id) from rate_card_books (NOLOCK)))

 

END

