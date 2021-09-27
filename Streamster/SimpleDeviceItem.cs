using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamster
{
    class SimpleDeviceItem
    {
        // ComboBox lags a lot if populted with an MMDeviceCollection. This is the only data we really need, though.
        public string Name { get; set; }
        public string ID { get; set; }
    }
}
