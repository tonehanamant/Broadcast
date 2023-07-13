select * from spot_exceptions_out_of_spec_decisions

select * from spot_exceptions_out_of_specs

update spot_exceptions_out_of_spec_decisions set synced_at=null , synced_by=null where id=1

select COUNT(*) as decisionCount from spot_exceptions_out_of_spec_decisions where synced_at is null

update spot_exceptions_out_of_spec_decisions set synced_at='3-24-2022'  where id=1