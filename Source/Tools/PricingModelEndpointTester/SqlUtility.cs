using System;
using System.Text;
using Services.Broadcast.Entities.Plan.Pricing;

namespace PricingModelEndpointTester
{
    public static class SqlUtility
    {
        public static string TransformResultToSql(PlanPricingApiSpotsResponseDto_v3 toTransform, string seed = "")
        {
            var sqlSb = new StringBuilder();
            var tableName = $"#AllocatedSpots_{seed}";

            var createTableSql = $"CREATE TABLE {tableName} ( id int, week_id int, spot_length_id int, frequency int);";
            sqlSb.AppendLine(createTableSql);

            foreach (var result in toTransform.Results)
            {
                foreach (var frequency in result.Frequencies)
                {
                    var insertSql = $"INSERT INTO {tableName} (id, week_id, spot_length_id, frequency) " +
                                    $"VALUES ({result.ManifestId}, {result.MediaWeekId}, {frequency.SpotLengthId}, {frequency.Frequency});";
                    sqlSb.AppendLine(insertSql);
                }
            }

            sqlSb.AppendLine("GO");
            return sqlSb.ToString();
        }
    }
}