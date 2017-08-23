using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Microsoft.VisualBasic.FileIO;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Services.Broadcast.Converters
{
    public interface IStationContactMasterFileImporter : IApplicationService
    {
        StringBuilder ExtractStationContactScript(Stream rawStream, string userName);
    }

    public class StationContactMasterFileImporter: IStationContactMasterFileImporter
    {
        private static readonly List<string> CsvFileHeaders = new List<string>()
        {
            "Market Rank",
            "Call Letters",
            "Affilation",
            "DMA",
            "Traffic Contact First Name",
            "Traffic Contact Phone",
            "Traffic Contact Email",
            "Traffic Contact Fax",
            "Station/Group Identifier"
        };

        private Dictionary<string, int> _requiredFields;

        private readonly IStationRepository _StationRepository;

        public StationContactMasterFileImporter(IDataRepositoryFactory broadcastDataRepositoryFactory)
        {
            _StationRepository = broadcastDataRepositoryFactory.GetDataRepository<IStationRepository>();
        }

        public StringBuilder ExtractStationContactScript(Stream rawStream, string userName)
        {
            var stationContacts = new List<StationContact>();

            using (var parser = _SetupCSVParser(rawStream))
            {
                _requiredFields = _ValidateAndSetupHeaders(parser);

                stationContacts.AddRange(_BuildStationContactsList(parser));
            }

            return _GenerateInsertScriptForContacts(stationContacts);
        }

        private StringBuilder _GenerateInsertScriptForContacts(List<StationContact> stationContacts)
        {
            var sql = new StringBuilder();
            sql.AppendLine("begin");
            var currentTime = DateTime.Now;
            foreach (var stationContact in stationContacts)
            {
                string sqlInsert =
                    string.Format(                                                                                                                                                                                                                                                           //[name],[phone],[fax],[email],[type],[station_code],[created_by],[created_date],[modified_by],[modified_date]                                                                            
                        "IF NOT EXISTS(select * from [dbo].[station_contacts] where station_code = {0} and email = '{1}') begin \n   INSERT INTO [dbo].[station_contacts] ([name],[phone],[fax],[email],[type],[station_code],[created_by],[created_date],[modified_by],[modified_date]) VALUES ('{2}','{3}','{4}','{5}',{6},{7},{8},'{9}',{10},'{11}') \n end \n ",
                        stationContact.StationCode, stationContact.Email, stationContact.Name, stationContact.Phone, stationContact.Fax, stationContact.Email,
                        (byte)stationContact.Type, stationContact.StationCode, "SYSTEM_USER", currentTime,
                        "SYSTEM_USER", currentTime);

                sql.AppendLine(sqlInsert);
            }

            sql.AppendLine("end");
            sql.AppendLine("go");

            return sql;
        }

        private IEnumerable<StationContact> _BuildStationContactsList(TextFieldParser parser)
        {
            var listOfContacts = new List<StationContact>();
            while (!parser.EndOfData)
            {
                var currentData = parser.ReadFields();
                if (_IsEmptyLine(currentData))
                {
                    continue; //skip line if empty
                }

                var contacts = _BuildStationContact(currentData);

                // remove duplica entries
                foreach (var contact in contacts)
                {
                    if (
                        !listOfContacts.Any(
                            a =>
                                a.StationCode == contact.StationCode &&
                                string.Compare(a.Email, contact.Email, StringComparison.OrdinalIgnoreCase) == 0))
                        listOfContacts.Add(contact);
                }
            }
            return listOfContacts;
        }

        private List<StationContact> _BuildStationContact(string[] currentData)
        {
            var stationContacts = new List<StationContact>();

            var contactEmail = currentData[_requiredFields["Traffic Contact Email"]];
            var stationCode = _GetStationCode(currentData[_requiredFields["Call Letters"]]);
            var contactName = currentData[_requiredFields["Traffic Contact First Name"]];
            var contactPhone = currentData[_requiredFields["Traffic Contact Phone"]];
            var contactFax = currentData[_requiredFields["Traffic Contact Fax"]];

            if (stationCode <= 0 || (string.IsNullOrWhiteSpace(contactEmail) && string.IsNullOrWhiteSpace(contactName)))
                return stationContacts;
            stationContacts.AddRange(contactEmail.Split(';').Select(email => new StationContact()
            {
                Email = email.Trim(),
                Type = StationContact.StationContactType.Traffic,
                StationCode = stationCode,
                Name = contactName,
                Phone = string.IsNullOrWhiteSpace(contactPhone) ? string.Empty : string.Join("", contactPhone.Take(63)),
                Fax = string.IsNullOrWhiteSpace(contactFax) ? string.Empty : string.Join("", contactFax.Take(63))
            }));

            return stationContacts;
        }

        private int _GetStationCode(string stationName)
        {
            var foundStation = _GetDisplayBroadcastStation(stationName);
            return foundStation == null ? 0 : foundStation.Code;
        }

        private DisplayBroadcastStation _GetDisplayBroadcastStation(string stationName)
        {
            return _StationRepository.GetBroadcastStationByLegacyCallLetters(stationName) ??
                                _StationRepository.GetBroadcastStationByCallLetters(stationName);
        }


        private Dictionary<string, int> _ValidateAndSetupHeaders(TextFieldParser parser)
        {
            var fields = parser.ReadFields().ToList();
            var headerDict = new Dictionary<string, int>();

            foreach (var header in CsvFileHeaders)
            {
                int headerItemIndex = fields.IndexOf(header);
                if (headerItemIndex < 0) continue;
                headerDict.Add(header, headerItemIndex);
            }
            return headerDict;
        }


        private TextFieldParser _SetupCSVParser(Stream rawStream)
        {
            var parser = new TextFieldParser(rawStream);
            if (parser.EndOfData)
            {
                throw new Exception("Cannot parse empty contact file.");
            }

            parser.SetDelimiters(new string[] { "," });

            return parser;
        }

        private bool _IsEmptyLine(string[] fieldArray)
        {
            foreach (var field in fieldArray)
            {
                if (!String.IsNullOrEmpty(field))
                {
                    return false;
                }
            }
            return true;
        }

    }
}
