﻿
CREATE FUNCTION [dbo].[StringToInt] (@input AS VARCHAR(128))  
RETURNS INT AS  
	BEGIN 
		RETURN CAST(@input AS INT)
	END