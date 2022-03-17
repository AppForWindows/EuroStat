using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace EuroStat {
    public enum details {
        full,
        allstubs,
        referencestubs
    }
    public enum MetaDataListResource {
        dataflow,
        datastructure,
        codelist,
        conceptscheme
    }
    public enum CategoryResource {
        categoryscheme,
        categorisation
    }
    public enum DataflowResource {
        dataflow,
        datastructure,
        codelist,
        conceptscheme,
        contentconstraint
    }
    public enum DataflowReferences {
        empty,
        none,
        children,
        descendants
    }
    public enum DataflowDataDetail {
        empty,
        dataonly,
        serieskeysonly,
        nodata
    }
    public static class Dictionary {
        public static void Migrate() {
            using (DataContext db = new DataContext())
                try {
                    db.Database.Migrate();
                } catch { }
        }
        public static List<ApiBaseURI> ApiBaseEmpty {
            get {
                return new List<ApiBaseURI> {
                    new ApiBaseURI("Eurostat", "Eurostat", "https://ec.europa.eu/eurostat/online-help/public/en/API_01_Introduction_en/#APIBASE_URI", "https://ec.europa.eu/eurostat/api/dissemination", "ESTAT", "https://ec.europa.eu/eurostat/api/dissemination/sdmx/2.1/sdmx-rest.wadl"),
                    new ApiBaseURI("DG_COMP", "DG COMP", "https://ec.europa.eu/eurostat/online-help/public/en/API_01_Introduction_en/#APIBASE_URI", "https://webgate.ec.europa.eu/comp/redisstat/api/dissemination", "COMP", "https://webgate.ec.europa.eu/comp/redisstat/api/dissemination/sdmx/2.1/sdmx-rest.wadl"),
                    new ApiBaseURI("DG_EMPL", "DG EMPL", "https://ec.europa.eu/eurostat/online-help/public/en/API_01_Introduction_en/#APIBASE_URI", "https://webgate.ec.europa.eu/empl/redisstat/api/dissemination", "EMPL", "https://webgate.ec.europa.eu/empl/redisstat/api/dissemination/sdmx/2.1/sdmx-rest.wadl"),
                    new ApiBaseURI("DG_GROW", "DG GROW", "https://ec.europa.eu/eurostat/online-help/public/en/API_01_Introduction_en/#APIBASE_URI", "https://webgate.ec.europa.eu/grow/redisstat/api/dissemination", "GROW", "https://webgate.ec.europa.eu/grow/redisstat/api/dissemination/sdmx/2.1/sdmx-rest.wadl")
                };
            }
        }
        public static List<ApiBaseURI> ApiBaseList {
            get {
                if (ABL == null || ABL.Count == 0)
                    using (DataContext context = new DataContext())
                        try {
                            ABL = context.ApiBaseURIes.ToList();
                            foreach (ApiBaseURI a in ABL)
                                a.LoadDB();
                        } catch (Exception gl) { }
                return ABL;
            }
        }
        private static List<ApiBaseURI> ABL = new List<ApiBaseURI>();
    }
}
