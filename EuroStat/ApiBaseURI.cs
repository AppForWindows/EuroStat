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

        public DataSet ds_CategoryScheme;
        public DataSet ds_Categorysation;
        public DataSet ds_Dataflow;
        public virtual DataTable CategoryScheme { get { return ds_CategoryScheme != null && ds_CategoryScheme.Tables != null ? ds_CategoryScheme.Tables["CategoryScheme"] : null; } }
        public virtual string CategorySchemeName { get { return "CategorySchemeName"; } }
        public virtual DataTable Category { get { return ds_CategoryScheme != null && ds_CategoryScheme.Tables != null ? ds_CategoryScheme.Tables["Category"] : null; } }
        public virtual string CategoryName { get { return "CategoryName"; } }
        public virtual string CategoryKeyFieldName { get { return ds_CategoryScheme != null && ds_CategoryScheme.Relations != null && ds_CategoryScheme.Relations["Category_Category"] != null ? ds_CategoryScheme.Relations["Category_Category"].ParentColumns[0].ColumnName : string.Empty; } }
        public virtual string CategoryParentFieldName { get { return ds_CategoryScheme != null && ds_CategoryScheme.Relations != null && ds_CategoryScheme.Relations["Category_Category"] != null ? ds_CategoryScheme.Relations["Category_Category"].ChildColumns[0].ColumnName : string.Empty; } }
        public virtual DataTable Categorisation { get { return ds_Categorysation != null && ds_Categorysation.Tables != null ? ds_Categorysation.Tables["Categorisation"] : null; } }
        public virtual DataTable Dataflow { get { return ds_Dataflow != null && ds_Dataflow.Tables != null ? ds_Dataflow.Tables["Dataflow"] : null; } }

        public void ClearDataSet() { ds_CategoryScheme = null; ds_Categorysation = null; ds_Dataflow = null; }

        public virtual string MetaDataListURI(MetaDataListResource MTLR, details D, bool completestubs) {
            return string.Format(@"{0}/sdmx/2.1/{1}/{2}/all?detail={3}{4}", api_base_uri, MTLR.ToString(), agencyID, D.ToString(), completestubs ? "&completestub=true" : "");
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
            ds_Dataflow = ds;
            if (ds == null || ds.Tables.Count == 0) return;
            if (MTLR == MetaDataListResource.dataflow) {
                ds.Tables["Dataflow"].Columns.Add("DataflowName", typeof(string));
                ds.Tables["Dataflow"].Columns.Add("DataflowDescription", typeof(string));
                ds.Tables["Dataflow"].Columns.Add("DataflowHTML", typeof(string));
                ds.Tables["Dataflow"].Columns.Add("DataflowSDMX", typeof(string));
                foreach (DataRow Df in ds.Tables["Dataflow"].Rows) {
                    DataRow name = Df.GetChildRows("Dataflow_Name").FirstOrDefault(n => n["lang"].ToString() == "en");
                    if (name != null)
                        Df["DataflowName"] = name["Name_Text"];
                    if (ds.Tables.Contains("Description") && ds.Tables["Description"].Columns.Contains("Description_Text")) {
                        DataRow desc = Df.GetChildRows("Dataflow_Description").FirstOrDefault(d => d["lang"].ToString() == "en");
                        if (desc != null)
                            Df["DataflowDescription"] = desc["Description_Text"];
                    }
                    if (ds.Tables.Contains("Annotation") && ds.Tables["Annotation"].Columns.Contains("AnnotationURL")) {
                        DataRow html = Df.GetChildRows("Dataflow_Annotation").FirstOrDefault(d => d["AnnotationType"].ToString() == "ESMS_HTML");
                        if (html != null)
                            Df["DataflowHTML"] = html["AnnotationURL"];
                        DataRow sdmx = Df.GetChildRows("Dataflow_Annotation").FirstOrDefault(d => d["AnnotationType"].ToString() == "ESMS_SDMX");
                        if (sdmx != null)
                            Df["DataflowSDMX"] = sdmx["AnnotationURL"];
                    }
                }
            }
        }
        public virtual async Task<DataSet> MetaDataListAsync(MetaDataListResource MTLR, details D, bool completestubs) {
             return await Components.GetDataSetAsync(MetaDataListURI(MTLR, D, completestubs), delegate (DataSet ds) { MetaDataListPrepare(ds, MTLR, D, completestubs); });
        }

        public virtual void DataflowUpdateBegin(Components.DataflowUpdated DfU, Components.DataSetDownloaded DSD) {
            if (ds_Dataflow == null || Dataflow == null || Dataflow.Rows.Count == 0) return;
            Components.BeginLoadDataSet(MetaDataListURI(MetaDataListResource.dataflow, details.allstubs, true), null, DSD, delegate (DataSet ds) { DataflowUpdatePrepare(ds, DfU); });
        }
        public virtual void DataflowUpdatePrepare(DataSet ds, Components.DataflowUpdated DfU) {
            if (ds == null || ds.Tables.Count == 0) return;
            foreach (DataRow D in Dataflow.Rows) {
                LoadDataRowFromDS(D, ds);
                if (DfU != null) DfU.Invoke(D);
            }
        }
        public virtual async void DataflowUpdateAsync(Components.DataflowUpdated DfU) {
            foreach (DataRow D in Dataflow.Rows) {
                await Components.GetDataSetAsync(string.Format(@"{0}/sdmx/2.1/dataflow/{1}/{2}?detail=allstubs&completestub=true", api_base_uri, agencyID, D["id"]), delegate (DataSet ds) { DataflowUpdatePrepare(ds, D); });
                if (DfU != null) DfU.Invoke(D);
            }
        }
        public virtual void DataflowUpdatePrepare(DataSet ds, DataRow D) {
            if (ds == null || ds.Tables.Count == 0) return;
            LoadDataRowFromDS(D, ds);
        }
        void LoadDataRowFromDS(DataRow D, DataSet ds) {
            foreach (DataRow Df in ds.Tables["Dataflow"].Select("id='" + D["id"].ToString() + "'")) {
                DataRow name = Df.GetChildRows("Dataflow_Name").FirstOrDefault(n => n["lang"].ToString() == "en");
                if (name != null)
                    D["DataflowName"] = name["Name_Text"];
                if (ds.Tables.Contains("Description") && ds.Tables["Description"].Columns.Contains("Description_Text")) {
                    DataRow desc = Df.GetChildRows("Dataflow_Description").FirstOrDefault(d => d["lang"].ToString() == "en");
                    if (desc != null)
                        D["DataflowDescription"] = desc["Description_Text"];
                } else if (D["DataflowDescription"] == DBNull.Value)
                    D["DataflowDescription"] = string.Empty;
                if (ds.Tables.Contains("Annotation") && ds.Tables["Annotation"].Columns.Contains("AnnotationURL")) {
                    DataRow html = Df.GetChildRows("Dataflow_Annotation").FirstOrDefault(d => d["AnnotationType"].ToString() == "ESMS_HTML");
                    if (html != null)
                        D["DataflowHTML"] = html["AnnotationURL"];
                    else if (D["DataflowHTML"] == DBNull.Value)
                        D["DataflowHTML"] = string.Empty;
                    DataRow sdmx = Df.GetChildRows("Dataflow_Annotation").FirstOrDefault(d => d["AnnotationType"].ToString() == "ESMS_SDMX");
                    if (sdmx != null)
                        D["DataflowSDMX"] = sdmx["AnnotationURL"];
                    else if (D["DataflowSDMX"] == DBNull.Value)
                        D["DataflowSDMX"] = string.Empty;
                } else {
                    if (D["DataflowHTML"] == DBNull.Value)
                        D["DataflowHTML"] = string.Empty;
                    if (D["DataflowSDMX"] == DBNull.Value)
                        D["DataflowSDMX"] = string.Empty;
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
            if (CR == CategoryResource.categoryscheme)
                ds_CategoryScheme = ds;
            else if (CR == CategoryResource.categorisation)
                ds_Categorysation = ds;
            if (ds == null || ds.Tables.Count == 0) return;
            if (CR == CategoryResource.categoryscheme)
                try {
                    CategoryScheme.Columns.Add(CategorySchemeName, typeof(string));
                    CategoryScheme.Columns.Add("COLOR_ICON", typeof(byte[]));
                    CategoryScheme.Columns.Add("GREY_ICON", typeof(byte[]));
                    CategoryScheme.Columns.Add("HOVER_ICON", typeof(byte[]));
                    foreach (DataRow CS in CategoryScheme.Rows) try {
                            DataRow name = CS.GetChildRows("CategoryScheme_Name").FirstOrDefault(n => n["lang"].ToString() == "en");
                            if (name != null)
                                CS[CategorySchemeName] = name["Name_Text"];
                            DataRow Ic = CS.GetChildRows("CategoryScheme_Annotation").FirstOrDefault(n => n["AnnotationType"].ToString() == "DISSEMINATION_COLOR_ICON");
                            if (Ic != null) 
                                CS["COLOR_ICON"] = Convert.FromBase64String(Ic["AnnotationTitle"].ToString());
                            Ic = CS.GetChildRows("CategoryScheme_Annotation").FirstOrDefault(n => n["AnnotationType"].ToString() == "DISSEMINATION_GREY_ICON");
                            if (Ic != null)
                                CS["GREY_ICON"] = Convert.FromBase64String(Ic["AnnotationTitle"].ToString());
                            Ic = CS.GetChildRows("CategoryScheme_Annotation").FirstOrDefault(n => n["AnnotationType"].ToString() == "DISSEMINATION_HOVER_ICON");
                            if (Ic != null)
                                CS["HOVER_ICON"] = Convert.FromBase64String(Ic["AnnotationTitle"].ToString());
                        } catch(Exception cs) { }

                    Category.Columns.Add(CategoryName, typeof(string));
                    foreach (DataRow C in Category.Rows) {
                        DataRow name = C.GetChildRows("Category_Name").FirstOrDefault(n => n["lang"].ToString() == "en");
                        if (name != null)
                            C[CategoryName] = name["Name_Text"];
                    }
                    foreach (DataRow C in Category.Select("CategoryScheme_Id is not null"))
                        SetCategoryScheme(C, C["CategoryScheme_Id"]);
                } catch { }
            else if (CR == CategoryResource.categorisation)
                try {
                    Categorisation.Columns.Add("SourceID", typeof(string));
                    Categorisation.Columns.Add("TargetID", typeof(string));
                    Categorisation.Columns.Add("TargetParentID", typeof(string));
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
                } catch { }
        }
        private void SetCategoryScheme(DataRow r, object id) {
            if (r["CategoryScheme_Id"] == DBNull.Value)
                r["CategoryScheme_Id"] = id;
            foreach (DataRow cr in r.GetChildRows("Category_Category"))
                SetCategoryScheme(cr, id);
        }
        public virtual async Task<DataSet> CategoryListAsync(CategoryResource CR) {
            return await Components.GetDataSetAsync(CategoryListURI(CR), delegate (DataSet ds) { CategoryListPrepare(ds, CR); });
        }

        //public virtual string DataflowURI(string ID, DataflowResource DR, string ver, DataflowReferences references, bool compressed) {
        //    List<string> param = new List<string> { references != DataflowReferences.empty ? "references=" + references.ToString() : "", compressed ? "compressed=true" : "" };//lang=en
        //    param.RemoveAll(p => string.IsNullOrWhiteSpace(p));
        //    return string.Format(@"{0}/sdmx/2.1/{1}/{2}/{3}{4}{5}", api_base_uri, DR.ToString(), agencyID, ID, !string.IsNullOrWhiteSpace(ver) ? "/" + ver : "", param.Count > 0 ? "?" + string.Join("&", param) : "");
        //}
        //public virtual DataSet DataflowList(string ID, DataflowResource DR, string ver, DataflowReferences references, bool compressed, IProgress<decimal> PR) {
        //    return Components.GetDataSet(DataflowURI(ID, DR, ver, references, compressed), PR);
        //}
        //public virtual void DataflowListBegin(string ID, DataflowResource DR, string ver, DataflowReferences references, bool compressed, Components.DataSetDownloadProgress DDP, Components.DataSetDownloaded DSD) {
        //    Components.BeginLoadDataSet(DataflowURI(ID, DR, ver, references, compressed), DDP, DSD, delegate (DataSet ds) { DataflowListPrepare(ds, ID, DR, ver, references, compressed); });
        //}
        //public virtual void DataflowListPrepare(DataSet ds, string ID, DataflowResource DR, string ver, DataflowReferences references, bool compressed) {
        //    if (ds == null || ds.Tables.Count == 0) return;
            
        //}
        //public virtual async Task<DataSet> DataflowListAsync(string ID, DataflowResource DR, string ver, DataflowReferences references, bool compressed) {
        //    return await Components.GetDataSetAsync(DataflowURI(ID, DR, ver, references, compressed), delegate (DataSet ds) { DataflowListPrepare(ds, ID, DR, ver, references, compressed); });
        //}

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
        public virtual async Task<DataSet> DataflowDataAsync(string ID, DataflowDataDetail DDD, bool compressed) {
            return await Components.GetDataSetAsync(DataflowDataURI(ID, DDD, compressed), delegate (DataSet ds) { DataflowDataPrepare(ds, ID, DDD, compressed); });
        }
    }
}
