using Services.Broadcast.Entities.Plan;
using System.Collections.Generic;
using System.Linq;
using Services.Broadcast.Entities;

namespace Services.Broadcast.IntegrationTests.TestData
{
    public static class AudienceTestData
    {
        public static List<PlanAudienceDisplay> GetAudiences()
        {
            #region Big List - Audiences
            var audiences = new List<PlanAudienceDisplay>
            {
                new PlanAudienceDisplay {Id = 4, Display = "Women 12-14", Code = "W12-14"},
                new PlanAudienceDisplay {Id = 5, Display = "Women 15-17", Code = "W15-17"},
                new PlanAudienceDisplay {Id = 6, Display = "Women 18-20", Code = "W18-20"},
                new PlanAudienceDisplay {Id = 7, Display = "Women 21-24", Code = "W21-24"},
                new PlanAudienceDisplay {Id = 13, Display = "Women 50-54", Code = "W50-54"},
                new PlanAudienceDisplay {Id = 14, Display = "Women 55-64", Code = "W55-64"},
                new PlanAudienceDisplay {Id = 15, Display = "Women 65+", Code = "W65+"},
                new PlanAudienceDisplay {Id = 19, Display = "Men 12-14", Code = "M12-14"},
                new PlanAudienceDisplay {Id = 20, Display = "Men 15-17", Code = "M15-17"},
                new PlanAudienceDisplay {Id = 21, Display = "Men 18-20", Code = "M18-20"},
                new PlanAudienceDisplay {Id = 22, Display = "Men 21-24", Code = "M21-24"},
                new PlanAudienceDisplay {Id = 28, Display = "Men 50-54", Code = "M50-54"},
                new PlanAudienceDisplay {Id = 29, Display = "Men 55-64", Code = "M55-64"},
                new PlanAudienceDisplay {Id = 30, Display = "Men 65+", Code = "M65+"},
                new PlanAudienceDisplay {Id = 31, Display = "Households", Code = "HH"},
                new PlanAudienceDisplay {Id = 32, Display = "Adults 18-34", Code = "A18-34"},
                new PlanAudienceDisplay {Id = 33, Display = "Adults 18-49", Code = "A18-49"},
                new PlanAudienceDisplay {Id = 34, Display = "Adults 18+", Code = "A18+"},
                new PlanAudienceDisplay {Id = 35, Display = "Adults 25-34", Code = "A25-34"},
                new PlanAudienceDisplay {Id = 36, Display = "Adults 25-49", Code = "A25-49"},
                new PlanAudienceDisplay {Id = 37, Display = "Adults 25-54", Code = "A25-54"},
                new PlanAudienceDisplay {Id = 38, Display = "Adults 35-49", Code = "A35-49"},
                new PlanAudienceDisplay {Id = 39, Display = "Adults 35-54", Code = "A35-54"},
                new PlanAudienceDisplay {Id = 40, Display = "Adults 35-64", Code = "A35-64"},
                new PlanAudienceDisplay {Id = 41, Display = "Adults 35+", Code = "A35+"},
                new PlanAudienceDisplay {Id = 42, Display = "Adults 50+", Code = "A50+"},
                new PlanAudienceDisplay {Id = 43, Display = "Adults 55+", Code = "A55+"},
                new PlanAudienceDisplay {Id = 44, Display = "Adults 65+", Code = "A65+"},
                new PlanAudienceDisplay {Id = 46, Display = "Children 6-11", Code = "C6-11"},
                new PlanAudienceDisplay {Id = 48, Display = "Men 18-34", Code = "M18-34"},
                new PlanAudienceDisplay {Id = 49, Display = "Men 18-49", Code = "M18-49"},
                new PlanAudienceDisplay {Id = 50, Display = "Men 18-54", Code = "M18-54"},
                new PlanAudienceDisplay {Id = 51, Display = "Men 25-54", Code = "M25-54"},
                new PlanAudienceDisplay {Id = 52, Display = "Men 35-54", Code = "M35-54"},
                new PlanAudienceDisplay {Id = 53, Display = "Men 35-64", Code = "M35-64"},
                new PlanAudienceDisplay {Id = 54, Display = "Men 35+", Code = "M35+"},
                new PlanAudienceDisplay {Id = 55, Display = "Women 18-34", Code = "W18-34"},
                new PlanAudienceDisplay {Id = 56, Display = "Women 18-49", Code = "W18-49"},
                new PlanAudienceDisplay {Id = 57, Display = "Women 25-49", Code = "W25-49"},
                new PlanAudienceDisplay {Id = 58, Display = "Women 25-54", Code = "W25-54"},
                new PlanAudienceDisplay {Id = 59, Display = "Women 35-54", Code = "W35-54"},
                new PlanAudienceDisplay {Id = 60, Display = "Women 35-64", Code = "W35-64"},
                new PlanAudienceDisplay {Id = 61, Display = "Women 35+", Code = "W35+"},
                new PlanAudienceDisplay {Id = 62, Display = "Women 50+", Code = "W50+"},
                new PlanAudienceDisplay {Id = 247, Display = "Adults 18-54", Code = "A18-54"},
                new PlanAudienceDisplay {Id = 249, Display = "Adults 21-24", Code = "A21-24"},
                new PlanAudienceDisplay {Id = 252, Display = "Adults 21-49", Code = "A21-49"},
                new PlanAudienceDisplay {Id = 257, Display = "Adults 25-64", Code = "A25-64"},
                new PlanAudienceDisplay {Id = 258, Display = "Adults 25+", Code = "A25+"},
                new PlanAudienceDisplay {Id = 264, Display = "Adults 50-54", Code = "A50-54"},
                new PlanAudienceDisplay {Id = 266, Display = "Adults 55-64", Code = "A55-64"},
                new PlanAudienceDisplay {Id = 277, Display = "Men 18+", Code = "M18+"},
                new PlanAudienceDisplay {Id = 280, Display = "Men 21-49", Code = "M21-49"},
                new PlanAudienceDisplay {Id = 284, Display = "Men 25-34", Code = "M25-34"},
                new PlanAudienceDisplay {Id = 286, Display = "Men 25-49", Code = "M25-49"},
                new PlanAudienceDisplay {Id = 287, Display = "Men 25-64", Code = "M25-64"},
                new PlanAudienceDisplay {Id = 288, Display = "Men 25+", Code = "M25+"},
                new PlanAudienceDisplay {Id = 290, Display = "Men 35-49", Code = "M35-49"},
                new PlanAudienceDisplay {Id = 295, Display = "Men 50+", Code = "M50+"},
                new PlanAudienceDisplay {Id = 296, Display = "Men 55+", Code = "M55+"},
                new PlanAudienceDisplay {Id = 306, Display = "Women 18-54", Code = "W18-54"},
                new PlanAudienceDisplay {Id = 308, Display = "Women 18+", Code = "W18+"},
                new PlanAudienceDisplay {Id = 312, Display = "Women 21-49", Code = "W21-49"},
                new PlanAudienceDisplay {Id = 318, Display = "Women 25-64", Code = "W25-64"},
                new PlanAudienceDisplay {Id = 319, Display = "Women 25+", Code = "W25+"},
                new PlanAudienceDisplay {Id = 329, Display = "Women 55+", Code = "W55+"},
                new PlanAudienceDisplay {Id = 339, Display = "Children 2-5", Code = "C2-5"},
                new PlanAudienceDisplay {Id = 346, Display = "O0+", Code = "O0+"},
                new PlanAudienceDisplay {Id = 347, Display = "Women 35-49", Code = "W35-49"},
                new PlanAudienceDisplay {Id = 348, Display = "Women 25-34", Code = "W25-34"},
                new PlanAudienceDisplay {Id = 416, Display = "Persons 12-14", Code = "P12-14"},
                new PlanAudienceDisplay {Id = 417, Display = "Persons 15-17", Code = "P15-17"},
                new PlanAudienceDisplay {Id = 418, Display = "Adults 18-20", Code = "A18-20"},
                new PlanAudienceDisplay {Id = 419, Display = "Persons 2+", Code = "P2+"},
                new PlanAudienceDisplay {Id = 420, Display = "Children 2-11", Code = "C2-11"},
                new PlanAudienceDisplay {Id = 421, Display = "Children 12-17", Code = "C12-17"},
                new PlanAudienceDisplay {Id = 422, Display = "Adults 18-24", Code = "A18-24"},
                new PlanAudienceDisplay {Id = 423, Display = "Adults 18-64", Code = "A18-64"},
                new PlanAudienceDisplay {Id = 424, Display = "Adults 50-64", Code = "A50-64"},
                new PlanAudienceDisplay {Id = 425, Display = "Men 18-24", Code = "M18-24"},
                new PlanAudienceDisplay {Id = 426, Display = "Men 18-64", Code = "M18-64"},
                new PlanAudienceDisplay {Id = 427, Display = "Men 50-64", Code = "M50-64"},
                new PlanAudienceDisplay {Id = 428, Display = "Women 18-24", Code = "W18-24"},
                new PlanAudienceDisplay {Id = 429, Display = "Women 18-64", Code = "W18-64"},
                new PlanAudienceDisplay {Id = 430, Display = "Women 50-64", Code = "W50-64"}
            };

            #endregion // #region Big List - Audiences

            return audiences;
        }

        public static PlanAudienceDisplay GetAudienceById(int id)
        {
            var audiences = GetAudiences();
            var found = audiences.First(a => a.Id == id);
            return found;
        }

        public static BroadcastAudience GetBroadcastAudienceById(int id)
        {
            var audiences = GetAllEntities();
            var found = audiences.First(a => a.Id == id);
            return found;
        }

        public static List<BroadcastAudience> GetAllEntities()
        {
            var audiences = GetAudiences();
            var entities = audiences.Select(a => new BroadcastAudience
            {
                Id = a.Id,
                Name = a.Display,
                Code = a.Code
            }).ToList();
            return entities;
        }
    }
}