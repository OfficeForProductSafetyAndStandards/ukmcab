using UKMCAB.Core.Domain.LegislativeAreas;

namespace UKMCAB.Core.Extensions
{
    public static class ProcedureModelExtensions
    {
        public static List<string> GetNamesByIds(this List<ProcedureModel> procedures, List<Guid> procedureIds)
        {
            var names = new List<string>();
            foreach (var procedureId in procedureIds)
            {
                var procedure = procedures.First(p => p.Id == procedureId);
                names.Add(procedure.Name);
            }
            return names;
        }
    }
}
