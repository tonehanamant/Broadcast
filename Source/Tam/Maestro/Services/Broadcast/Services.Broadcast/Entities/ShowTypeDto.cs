using System;
using Services.Broadcast.Entities.Enums;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.Entities
{
    public class ShowTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ProgramSourceEnum ShowTypeSource { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as ShowTypeDto);
        }

        public bool Equals(ShowTypeDto other)
        {
            return Id.Equals(other.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override string ToString()
        {
            return $"{Id} - {Name} - {ShowTypeSource.ToString()}";
        }

        public LookupDto ToLookupDto()
        {
            return new LookupDto
            {
                Id = Id,
                Display = Name,
            };
        }
    }
}
