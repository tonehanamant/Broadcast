CREATE PROCEDURE [dbo].[ups_REL_CreateHouseIsciiForMaterial]
(
      @year int,
      @material_id int
)
AS
BEGIN
      declare @houseiscii varchar(8);
      declare @doesexist int;
      declare @lastseed int;
      declare @defmarker varchar(1);
      declare @padnumber varchar(4);
      
      BEGIN TRANSACTION
      SELECT @houseiscii = mhm.houseiscii from materials_houseiscii_map mhm with (NOLOCK) where
            mhm.material_id = @material_id;
      
      
      IF(@houseiscii IS NULL)
      BEGIN
            SELECT @doesexist = COUNT(*) from houseiscii_seed hs WITH (NOLOCK) WHERE
                  hs.year = @year;
      
            
            IF(@doesexist = 0)
            BEGIN
                  INSERT INTO houseiscii_seed (YEAR) VALUES (@year);
                  set @lastseed = -1;
            END
            ELSE
            BEGIN
                  SELECT @lastseed = MAX(hs.seednumber) from houseiscii_seed hs WITH (NOLOCK) WHERE hs.year = @year;
            END
            
            
            SELECT @defmarker = CASE WHEN m.is_hd = 1 THEN 'H' ELSE 'S' END FROM materials m with (NOLOCK) where m.id = @material_id;
            
            SET @lastseed = @lastseed + 1;
      
            UPDATE houseiscii_seed SET SEEDNUMBER = @lastseed WHERE YEAR = @year;
            
            SET @padnumber = STUFF(@lastseed, 1, 0, REPLICATE('0', 4 - LEN(@lastseed)));
            
            INSERT INTO materials_houseiscii_map(material_id, houseiscii) VALUES
                  (@material_id, 'ET' + 
                  CASE 
                        WHEN @year = 2010 THEN 'A'
                        WHEN @year = 2011 THEN 'B'
                        WHEN @year = 2012 THEN 'C' 
                        WHEN @year = 2013 THEN 'D'
                        WHEN @year = 2014 THEN 'E'
                        WHEN @year = 2015 THEN 'F'
                        WHEN @year = 2016 THEN 'G'
                        WHEN @year = 2017 THEN 'H'
                        WHEN @year = 2018 THEN 'I'
                        WHEN @year = 2019 THEN 'J'
                        WHEN @year = 2020 THEN 'K'
                        WHEN @year = 2021 THEN 'L'
                        WHEN @year = 2022 THEN 'M'
                        WHEN @year = 2023 THEN 'N'
                        WHEN @year = 2024 THEN 'P'
                        WHEN @year = 2025 THEN 'Q'
                        WHEN @year = 2026 THEN 'R'
                        WHEN @year = 2027 THEN 'S'
                        WHEN @year = 2028 THEN 'T'
                        WHEN @year = 2029 THEN 'U'
                        WHEN @year = 2030 THEN 'V'
                        WHEN @year = 2031 THEN 'W'
                        WHEN @year = 2032 THEN 'X'
                        WHEN @year = 2033 THEN 'Y'
                        WHEN @year = 2034 THEN 'Z'
                  END
                  + @padnumber
                  + @defmarker);
                  
                  SELECT @houseiscii = mhm.houseiscii from materials_houseiscii_map mhm with (NOLOCK) where
                  mhm.material_id = @material_id;
      END
      ELSE
      BEGIN
            -- UPDATE THE DEF MARKER JUST IN CASE
            SELECT @defmarker = CASE WHEN m.is_hd = 1 THEN 'H' ELSE 'S' END FROM materials m with (NOLOCK) where m.id = @material_id;
            UPDATE materials_houseiscii_map SET houseiscii = SUBSTRING(houseiscii, 0, LEN(houseiscii))+ @defmarker
            WHERE material_id = @material_id;
            
            SELECT @houseiscii = mhm.houseiscii from materials_houseiscii_map mhm with (NOLOCK) where
                  mhm.material_id = @material_id;
            
      END
      COMMIT
END
