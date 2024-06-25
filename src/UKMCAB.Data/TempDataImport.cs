using Microsoft.AspNetCore.Hosting.Server.Features;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UKMCAB.Data
{
    public class TempDataImport
    {
        private readonly string _csvFilePath;

        public TempDataImport(string csvImportFilePath) => _csvFilePath = csvImportFilePath;

        public IEnumerable<ImportEntry> GetUKASReferences()
        {
            var lines = File.ReadAllLines(_csvFilePath)
                .Skip(1)  // first column is header!
                .Select(r=>r.Split(","))
                .ToList().Select(r=> new ImportEntry(r[0], r[1]));

            return lines;
        
        }
    }

    public record ImportEntry(string CabId, string UKASRef);
}
