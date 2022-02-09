using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public static List<Type> ApiBaseURITypes {
            get {
                if (ABUT == null || ABUT.Count == 0)
                    try {
                        ABUT = new List<Type>();
                        System.Reflection.Assembly cur = typeof(ApiBaseURI).Assembly;
                        foreach (System.Reflection.Assembly s in AppDomain.CurrentDomain.GetAssemblies().Where(s => s == cur || s.GetReferencedAssemblies().Any(a => a.FullName == cur.FullName)))
                            try {
                                ABUT.AddRange(s.GetTypes().Where(t => t.IsSubclassOf(typeof(ApiBaseURI)) && !t.IsAbstract));
                            } catch { }
                    } catch (Exception gt) { }
                return ABUT;
            }
        }
        private static List<Type> ABUT = new List<Type>();
    }
}
