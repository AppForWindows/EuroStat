using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Data;
using System.IO;
using System.ComponentModel;

namespace EuroStat {
    [DisplayName("ApiBaseURI"), Description("https://ec.europa.eu/eurostat/online-help/public/en/API_01_Introduction_en/#APIBASE_URI")]
    public abstract class ApiBaseURI {
        public string DisplayName { get { return Components.GetDisplayName(this.GetType()); } }
        public string Description { get { return Attribute.GetCustomAttribute(this.GetType(), typeof(DescriptionAttribute)) != null ? ((DescriptionAttribute)Attribute.GetCustomAttribute(this.GetType(), typeof(DescriptionAttribute))).Description : this.GetType().FullName; } }
        public abstract string api_base_uri { get; }
        public abstract string agencyID { get; }
        public abstract string catalogue { get; }

        //public virtual DataSet GetCatalogue() {
        //    return Components.GetDataSet(catalogue);
        //}
        //public virtual async Task<DataSet> GetCatalogueAsync() {
        //    return await Components.GetDataSetAsync(catalogue);
        //}

        public virtual string MetaDataListURI(MetaDataListResource MTLR, details D, bool completestubs) {
            return string.Format(@"{0}/sdmx/2.1/{1}/{2}/all?detail={3}{4}", api_base_uri, MTLR.ToString(), agencyID, D.ToString(), completestubs ? "&completestubs=true" : "");
        }
        public virtual DataSet MetaDataList(MetaDataListResource MTLR, details D, bool completestubs, IProgress<decimal> PR) {
            DataSet ds = Components.GetDataSet(MetaDataListURI(MTLR, D, completestubs), PR);
            MetaDataListPrepare(ds, MTLR, D, completestubs);
            return ds;
        }
        public virtual void MetaDataListBegin(MetaDataListResource MTLR, details D, bool completestubs, Components.DataSetDownloadProgress DDP, Components.DataSetDownloaded DSD) {
            Components.BeginLoadDataSet(MetaDataListURI(MTLR, D, completestubs), DDP, DSD, delegate (DataSet ds) { MetaDataListPrepare(ds, MTLR, D, completestubs); });
        }
        public virtual void MetaDataListPrepare(DataSet ds, MetaDataListResource MTLR, details D, bool completestubs) {
            if (ds == null || ds.Tables.Count == 0) return;
            if (MTLR == MetaDataListResource.dataflow) {
                ds.Tables["Dataflow"].Columns.Add("DataflowName", typeof(string));
                ds.Tables["Dataflow"].Columns.Add("DataflowDescription", typeof(string));
                foreach (DataRow Df in ds.Tables["Dataflow"].Rows) {
                    DataRow name = Df.GetChildRows("Dataflow_Name").FirstOrDefault(n => n["lang"].ToString() == "en");
                    if (name != null)
                        Df["DataflowName"] = name["Name_Text"];
                    DataRow desc = Df.GetChildRows("Dataflow_Description").FirstOrDefault(d => d["lang"].ToString() == "en");
                    if (desc != null)
                        Df["DataflowDescription"] = desc["Description_Text"];
                }
            }
        }

        public virtual string CategoryListURI(CategoryResource CR) {
            return string.Format(@"{0}/sdmx/2.1/{1}/{2}/all", api_base_uri, CR.ToString(), agencyID);
        }
        public virtual DataSet CategoryList(CategoryResource CR, IProgress<decimal> PR) {
            DataSet ds = Components.GetDataSet(CategoryListURI(CR), PR);
            CategoryListPrepare(ds, CR);
            return ds;
        }
        public virtual void CategoryListBegin(CategoryResource CR, Components.DataSetDownloadProgress DDP, Components.DataSetDownloaded DSD) {
            Components.BeginLoadDataSet(CategoryListURI(CR), DDP, DSD, delegate (DataSet ds) { CategoryListPrepare(ds, CR); });
        }
        public virtual void CategoryListPrepare(DataSet ds, CategoryResource CR) {
            if (ds == null || ds.Tables.Count == 0) return;
            if (CR == CategoryResource.categoryscheme) {
                ds.Tables["CategoryScheme"].Columns.Add("CategorySchemeName", typeof(string));
                foreach (DataRow CS in ds.Tables["CategoryScheme"].Rows) {
                    DataRow name = CS.GetChildRows("CategoryScheme_Name").FirstOrDefault(n => n["lang"].ToString() == "en");
                    if (name != null)
                        CS["CategorySchemeName"] = name["Name_Text"];
                }

                ds.Tables["Category"].Columns.Add("CategoryName", typeof(string));
                foreach (DataRow C in ds.Tables["Category"].Rows) {
                    DataRow name = C.GetChildRows("Category_Name").FirstOrDefault(n => n["lang"].ToString() == "en");
                    if (name != null)
                        C["CategoryName"] = name["Name_Text"];
                }
                foreach (DataRow C in ds.Tables["Category"].Select("CategoryScheme_Id is not null"))
                    SetCategoryScheme(C, C["CategoryScheme_Id"]);
            } else if (CR == CategoryResource.categorisation) {
                ds.Tables["Categorisation"].Columns.Add("SourceID", typeof(string));
                ds.Tables["Categorisation"].Columns.Add("TargetID", typeof(string));
                ds.Tables["Categorisation"].Columns.Add("TargetParentID", typeof(string));
                foreach (DataRow C in ds.Tables["Categorisation"].Rows) {
                    DataRow S = C.GetChildRows("Categorisation_SourceRef").FirstOrDefault();
                    if (S != null)
                        C["SourceID"] = S["id"];
                    DataRow T = C.GetChildRows("Categorisation_TargetRef").FirstOrDefault();
                    if (T != null) {
                        C["TargetID"] = T["id"];
                        C["TargetParentID"] = T["maintainableParentID"];
                    }
                }
            }
        }
        private void SetCategoryScheme(DataRow r, object id) {
            if (r["CategoryScheme_Id"] == DBNull.Value)
                r["CategoryScheme_Id"] = id;
            foreach (DataRow cr in r.GetChildRows("Category_Category"))
                SetCategoryScheme(cr, id);
        }

        public virtual string DataflowURI(string ID, DataflowResource DR, string ver, DataflowReferences references, bool compressed) {
            List<string> param = new List<string> { references != DataflowReferences.empty ? "references=" + references.ToString() : "", compressed ? "compressed=true" : "" };//lang=en
            param.RemoveAll(p => string.IsNullOrWhiteSpace(p));
            return string.Format(@"{0}/sdmx/2.1/{1}/{2}/{3}{4}{5}", api_base_uri, DR.ToString(), agencyID, ID, !string.IsNullOrWhiteSpace(ver) ? "/" + ver : "", param.Count > 0 ? "?" + string.Join("&", param) : "");
        }
        public virtual DataSet Dataflow(string ID, DataflowResource DR, string ver, DataflowReferences references, bool compressed, IProgress<decimal> PR) {
            return Components.GetDataSet(DataflowURI(ID, DR, ver, references, compressed), PR);
        }
        public virtual void DataflowBegin(string ID, DataflowResource DR, string ver, DataflowReferences references, bool compressed, Components.DataSetDownloadProgress DDP, Components.DataSetDownloaded DSD) {
            Components.BeginLoadDataSet(DataflowURI(ID, DR, ver, references, compressed), DDP, DSD, delegate (DataSet ds) { DataflowPrepare(ds, ID, DR, ver, references, compressed); });
        }
        public virtual void DataflowPrepare(DataSet ds, string ID, DataflowResource DR, string ver, DataflowReferences references, bool compressed) {
            if (ds == null || ds.Tables.Count == 0) return;
            
        }

        public virtual string DataflowDataURI(string ID, DataflowDataDetail DDD, bool compressed) {
            string key = "";
            List<string> param = new List<string> { "format=SDMX_2.1_STRUCTURED", DDD != DataflowDataDetail.empty ? "detail=" + DDD.ToString() : "", compressed ? "compressed=true" : "" };
            param.RemoveAll(p => string.IsNullOrWhiteSpace(p));
            return string.Format(@"{0}/sdmx/2.1/data/{1}{2}{3}", api_base_uri, ID, !string.IsNullOrWhiteSpace(key) ? "/" + key : "", param.Count > 0 ? "?" + string.Join("&", param) : "");
        }
        public virtual DataSet DataflowData(string ID, DataflowDataDetail DDD, bool compressed, IProgress<decimal> PR) {
            return Components.GetDataSet(DataflowDataURI(ID, DDD, compressed), PR);
        }
        public virtual void DataflowDataBegin(string ID, DataflowDataDetail DDD, bool compressed, Components.DataSetDownloadProgress DDP, Components.DataSetDownloaded DSD) {
            Components.BeginLoadDataSet(DataflowDataURI(ID, DDD, compressed), DDP, DSD, delegate (DataSet ds) { DataflowDataPrepare(ds, ID, DDD, compressed); });
        }
        public virtual void DataflowDataPrepare(DataSet ds, string ID, DataflowDataDetail DDD, bool compressed) {
            if (ds == null || ds.Tables.Count == 0) return;

        }
    }
}
