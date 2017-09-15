using System.Runtime.Remoting.Contexts;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using Services.Broadcast.Exceptions;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.Repositories
{
    public interface IStationContactsRepository : IDataRepository
    {
        List<StationContact> GetStationContactsByStationCode(int stationCode);
        List<StationContact> GetStationContactsByStationCode(List<int> stationCodes);
        void CreateNewStationContacts(List<StationContact> stationContacts, string user, int? fileId);
        void UpdateExistingStationContacts(List<StationContact> stationContacts, string user, int? fileId);
        station_contacts FindByStationContactId(int stationContactId);
        List<string> GetRepTeamNames();
        void DeleteStationContact(int stationContactId);
        List<StationContact> GetLatestContactsByName(string query);
    }

    public class StationContactsRepository : BroadcastRepositoryBase, IStationContactsRepository
    {
        public StationContactsRepository(ISMSClient pSmsClient, IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory, ITransactionHelper pTransactionHelper)
            : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper)
        {
        }

        public List<StationContact> GetStationContactsByStationCode(List<int> stationCodes)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return (from c in context.station_contacts
                        where stationCodes.Contains(c.station_code) 
                        select new StationContact()
                        {
                            Id = c.id,
                            Name = c.name,
                            Phone = c.phone,
                            Fax = c.fax,
                            Email = c.email,
                            Company = c.company,
                            StationCode = c.station_code,
                            Type = (StationContact.StationContactType) c.type
                        }).ToList();
                });
        }

        public List<StationContact> GetStationContactsByStationCode(int stationCode)
        {
            return GetStationContactsByStationCode(
                new List<int>()
                {
                    stationCode
                });
        }

        public List<StationContact> GetLatestContactsByName(string query)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var contactList = context.station_contacts.Where(c => c.name.ToLower().Contains(query.ToLower()))
                        .GroupBy(c => new {c.name, c.type, c.company})
                        .Select(g => g.OrderByDescending(c => c.modified_date).FirstOrDefault())
                        .Select(
                            c => new StationContact()
                            {
                                Id = c.id,
                                Company = c.company,
                                Email = c.email,
                                Fax = c.fax,
                                Name = c.name,
                                Phone = c.phone,
                                Type = (StationContact.StationContactType) c.type,
                                StationCode = c.station_code
                            }).OrderBy(c => c.Name).ToList();
                    return contactList;
                });
        }

        public void CreateNewStationContacts(List<StationContact> stationContacts, string user, int? fileId)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var timestamp = DateTime.Now;
                    foreach (var stationContact in stationContacts)
                    {
                        var dbContact = new station_contacts()
                        {
                            name = stationContact.Name,
                            phone = stationContact.Phone,
                            fax = stationContact.Fax,
                            email = stationContact.Email,
                            company = stationContact.Company,
                            type = (byte)stationContact.Type,
                            station_code = (short)stationContact.StationCode,
                            created_by = user,
                            created_date = timestamp,
                            modified_by = user,
                            modified_date = timestamp,
                            created_file_id = fileId
                        };
                        context.station_contacts.Add(dbContact);
                    }
                    context.SaveChanges();

                });
        }

        public void UpdateExistingStationContacts(List<StationContact> stationContacts, string user, int? fileId)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    foreach (var stationContact in stationContacts)
                    {
                        var dbContact = context.station_contacts.FirstOrDefault(c => c.id == stationContact.Id);
                        if (dbContact != null)
                        {
                            dbContact.name = stationContact.Name;
                            dbContact.phone = stationContact.Phone;
                            dbContact.email = stationContact.Email;
                            dbContact.fax = stationContact.Fax;
                            dbContact.company = stationContact.Company;
                            dbContact.type = (byte) stationContact.Type;
                            dbContact.modified_by = user;
                            dbContact.modified_date = DateTime.Now;
                            dbContact.modified_file_id = fileId;
                            dbContact.station_code = (short) stationContact.StationCode;
                            context.Entry(dbContact).State = EntityState.Modified;
                        }
                        else
                        {
                            throw new BroadcastRateDataException(string.Format("Can't find station contact to update: {0}-{1}", stationContact.Id, stationContact.Name), fileId.GetValueOrDefault());
                        }
                    }
                    context.SaveChanges();
                });
        }

        public List<string> GetRepTeamNames()
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return (from rt in context.rep_firms
                        select rt.team_name).ToList();
                });

        }

        public station_contacts FindByStationContactId(int stationContactId)
        {
            return _InReadUncommitedTransaction(context => context.station_contacts.FirstOrDefault(s=> s.id == stationContactId));
        }

        public void DeleteStationContact(int stationContactId)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var dbContact = context.station_contacts.FirstOrDefault(c => c.id == stationContactId);
                    if (dbContact == null) return;
                    context.station_contacts.Remove(dbContact);
                    context.SaveChanges();
                });
        }
    }
}
