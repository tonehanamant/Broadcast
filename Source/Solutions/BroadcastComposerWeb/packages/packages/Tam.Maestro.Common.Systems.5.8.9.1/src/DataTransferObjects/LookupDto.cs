using System;
using System.Runtime.Serialization;

namespace Tam.Maestro.Data.Entities.DataTransferObjects
{
    [DataContract]
    [Serializable]
    public class LookupDto
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string Display { get; set; }

        public LookupDto() { }

        public LookupDto(int id, string display)
        {
            this.Id = id;
            this.Display = display;
        }

        public override string ToString()
        {
            return Display;
        }

        public string ToErrorString()
        {
            return String.Format("{{Id: {0}, Display: {1}}}", Id, Display);
        }
    }
}

