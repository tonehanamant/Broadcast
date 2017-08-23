CREATE VIEW [dbo].[uvw_display_materials]
AS
SELECT 
	m.id 'material_id',
	m.spot_length_id,
	m.product_id,
	c.id 'advertiser_company_id',
	sl.length,
	pr.name 'product',
	c.name 'advertiser',
	m.code,
	m.title,
	m.type,
	m.phone_number,
	m.phone_type,
	m.date_received,
	m.date_created,
	m.date_last_modified,
	m.url,
	m.tape_log,
	m.active,
	m.is_hd,
	m.is_house_isci,
	m.has_screener,
	m_real.id 'client_material_id',
	m_real.code 'client_material_code',
	m.language_id
	
FROM 
	materials m					(NOLOCK)
	LEFT JOIN products pr		(NOLOCK) ON pr.id=m.product_id 
	LEFT JOIN companies c		(NOLOCK) ON c.id=pr.company_id 
	LEFT JOIN spot_lengths sl	(NOLOCK) ON sl.id=m.spot_length_id
	-- link to "real_material_id"
	LEFT JOIN materials m_real	(NOLOCK) ON m_real.id=m.real_material_id
