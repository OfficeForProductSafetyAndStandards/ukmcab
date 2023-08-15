using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UKMCAB.Data.Models
{
    public class LegacyDocument : Document
    {

        // Audit
        public Audit Created { get; set; }
        public Audit LastUpdated { get; set; }
        // Used by the search index, saves a lot of effort to flatten the model in the data source
        public Audit Published { get; set; }
        public Audit Archived { get; set; }
        public string ArchivedReason { get; set; }
    }

}
