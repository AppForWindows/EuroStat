using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace EuroStat {
    [DisplayName("Eurostat"), Description("https://ec.europa.eu/eurostat/online-help/public/en/API_01_Introduction_en/#APIBASE_URI")]
    public class Eurostat : ApiBaseURI {
        public override string api_base_uri => "https://ec.europa.eu/eurostat/api/dissemination";
        public override string agencyID => "ESTAT";
        public override string catalogue => "https://ec.europa.eu/eurostat/api/dissemination/sdmx/2.1/sdmx-rest.wadl";
    }
    [DisplayName("DG COMP"), Description("https://ec.europa.eu/eurostat/online-help/public/en/API_01_Introduction_en/#APIBASE_URI")]
    public class DG_COMP : ApiBaseURI {
        public override string api_base_uri => "https://webgate.ec.europa.eu/comp/redisstat/api/dissemination";
        public override string agencyID => "COMP";
        public override string catalogue => "https://webgate.ec.europa.eu/comp/redisstat/api/dissemination/sdmx/2.1/sdmx-rest.wadl";
    }
    [DisplayName("DG EMPL"), Description("https://ec.europa.eu/eurostat/online-help/public/en/API_01_Introduction_en/#APIBASE_URI")]
    public class DG_EMPL : ApiBaseURI {
        public override string api_base_uri => "https://webgate.ec.europa.eu/empl/redisstat/api/dissemination";
        public override string agencyID => "EMPL";
        public override string catalogue => "https://webgate.ec.europa.eu/empl/redisstat/api/dissemination/sdmx/2.1/sdmx-rest.wadl";
    }
    [DisplayName("DG GROW"), Description("https://ec.europa.eu/eurostat/online-help/public/en/API_01_Introduction_en/#APIBASE_URI")]
    public class DG_GROW : ApiBaseURI {
        public override string api_base_uri => "https://webgate.ec.europa.eu/grow/redisstat/api/dissemination";
        public override string agencyID => "GROW";
        public override string catalogue => "https://webgate.ec.europa.eu/grow/redisstat/api/dissemination/sdmx/2.1/sdmx-rest.wadl";
    }
}
