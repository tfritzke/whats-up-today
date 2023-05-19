using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhatsUpToday.Core.Data.Interfaces;

// See SAVE_CHANGES

public interface IAutoSaveEntityCreatedBy
{
    string CreatedBy { get; set; }
}
