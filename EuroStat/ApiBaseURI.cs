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
        public abstract string ID { get; }
        public string DisplayName { get { return Components.GetDisplayName(this.GetType()); } }
        public string Description { get { return Attribute.GetCustomAttribute(this.GetType(), typeof(DescriptionAttribute)) != null ? ((DescriptionAttribute)Attribute.GetCustomAttribute(this.GetType(), typeof(DescriptionAttribute))).Description : this.GetType().FullName; } }
        public abstract string api_base_uri { get; }
        public abstract string agencyID { get; }
        public abstract string catalogue { get; }

        public List<CategoryScheme> CategorySchemeList = null;
        public List<Category> CategoryList = null;
        public List<Categorisation> CategorisationList = null;
        public List<Dataflow> DataflowList = null;

        public void ClearDataSet() { CategorySchemeList = null; CategoryList = null; CategorisationList = null; DataflowList = null; }

        public virtual string DataflowListURI(MetaDataListResource MTLR, details D, bool completestubs) {
            return string.Format(@"{0}/sdmx/2.1/{1}/{2}/all?detail={3}{4}", api_base_uri, MTLR.ToString(), agencyID, D.ToString(), completestubs ? "&completestub=true" : "");
        }
        public virtual DataSet DataflowListGet(MetaDataListResource MTLR, details D, bool completestubs, IProgress<decimal> PR) {
            DataSet ds = Components.GetDataSet(DataflowListURI(MTLR, D, completestubs), PR);
            DataflowPrepare(ds, MTLR, D, completestubs);
            return ds;
        }
        public virtual void DataflowListBegin(MetaDataListResource MTLR, details D, bool completestubs, Components.DataSetDownloadProgress DDP, Components.DataSetDownloaded DSD) {
            Components.BeginLoadDataSet(DataflowListURI(MTLR, D, completestubs), DDP, DSD, delegate (DataSet ds) { DataflowPrepare(ds, MTLR, D, completestubs); });
        }
        public virtual void DataflowPrepare(DataSet ds, MetaDataListResource MTLR, details D, bool completestubs) {
            if (MTLR == MetaDataListResource.dataflow)
                DataflowList = new List<Dataflow>();
            if (ds == null || ds.Tables.Count == 0) return;
            if (MTLR == MetaDataListResource.dataflow)
                try {
                    foreach (DataRow Df in ds.Tables["Dataflow"].Rows)
                        DataflowList.Add(new Dataflow(this, Df["id"].ToString(), Df));
                } catch (Exception d) { }
        }
        public virtual async Task<DataSet> DataflowListAsync(MetaDataListResource MTLR, details D, bool completestubs) {
            return await Components.GetDataSetAsync(DataflowListURI(MTLR, D, completestubs), delegate (DataSet ds) { DataflowPrepare(ds, MTLR, D, completestubs); });
        }

        public virtual void DataflowUpdateBegin(Components.DataflowUpdated DfU, Components.DataSetDownloaded DSD) {
            if (DataflowList == null || DataflowList.Count == 0) return;
            Components.BeginLoadDataSet(DataflowListURI(MetaDataListResource.dataflow, details.allstubs, true), null, DSD, delegate (DataSet ds) { DataflowUpdatePrepare(ds, DfU); });
        }
        public virtual void DataflowUpdatePrepare(DataSet ds, Components.DataflowUpdated DfU) {
            if (ds == null || ds.Tables.Count == 0 || DataflowList == null || DataflowList.Count == 0) return;
            foreach (Dataflow Df in DataflowList) {
                Df.UpdateFromDS(ds);
                if (DfU != null) DfU.Invoke(Df);
            }
        }
        public virtual async void DataflowUpdateAsync(Components.DataflowUpdated DfU) {
            if (DataflowList == null || DataflowList.Count == 0) return;
            foreach (Dataflow Df in DataflowList) {
                await Components.GetDataSetAsync(string.Format(@"{0}/sdmx/2.1/dataflow/{1}/{2}?detail=allstubs&completestub=true", api_base_uri, agencyID, Df.ID), delegate (DataSet ds) { Df.UpdateFromDS(ds); });
                if (DfU != null) DfU.Invoke(Df);
            }
        }

        public virtual string CategoryListURI(CategoryResource CR) {
            return string.Format(@"{0}/sdmx/2.1/{1}/{2}/all", api_base_uri, CR.ToString(), agencyID);
        }
        public virtual DataSet CategoryListGet(CategoryResource CR, IProgress<decimal> PR) {
            DataSet ds = Components.GetDataSet(CategoryListURI(CR), PR);
            CategoryListPrepare(ds, CR);
            return ds;
        }
        public virtual void CategoryListBegin(CategoryResource CR, Components.DataSetDownloadProgress DDP, Components.DataSetDownloaded DSD) {
            Components.BeginLoadDataSet(CategoryListURI(CR), DDP, DSD, delegate (DataSet ds) { CategoryListPrepare(ds, CR); });
        }
        public virtual void CategoryListPrepare(DataSet ds, CategoryResource CR) {
            if (CR == CategoryResource.categoryscheme) {
                CategorySchemeList = new List<CategoryScheme>();
                CategoryList = new List<Category>();
            } else if (CR == CategoryResource.categorisation)
                CategorisationList = new List<Categorisation>();
            if (ds == null || ds.Tables.Count == 0) return;
            if (CR == CategoryResource.categoryscheme)
                try {
                    foreach (DataRow CS in ds.Tables["CategoryScheme"].Rows)
                        CategorySchemeList.Add(new CategoryScheme(this, CS["id"].ToString(), CS));

                    foreach (DataRow C in ds.Tables["Category"].Select("CategoryScheme_Id is not null"))
                        SetCategoryScheme(C, C["CategoryScheme_Id"]);
                    foreach (DataRow C in ds.Tables["Category"].Rows)
                        CategoryList.Add(new Category(this, C["id"].ToString(), C));
                } catch { }
            else if (CR == CategoryResource.categorisation)
                try {
                    foreach (DataRow C in ds.Tables["Categorisation"].Rows)
                        CategorisationList.Add(new Categorisation(this, C["id"].ToString(), C));
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

    public class CategoryScheme {
        public ApiBaseURI ApiBase { get; private set; }
        public string ApiBaseID { get; private set; }
        public string ID { get; private set; }
        public string Name { get; set; }
        public byte[] IconColor { get; set; }
        public byte[] IconGray { get; set; }
        public byte[] IconHover { get; set; }

        public CategoryScheme(ApiBaseURI _ApiBase, string _ID, DataRow drCategoryScheme) {
            ApiBase = _ApiBase;
            ApiBaseID = _ApiBase.ID;
            ID = _ID;
            try {
                DataRow name = drCategoryScheme.GetChildRows("CategoryScheme_Name").FirstOrDefault(n => n["lang"].ToString() == "en");
                if (name != null)
                    Name = name["Name_Text"].ToString();
                DataRow Ic = drCategoryScheme.GetChildRows("CategoryScheme_Annotation").FirstOrDefault(n => n["AnnotationType"].ToString() == "DISSEMINATION_COLOR_ICON");
                if (Ic != null)
                    IconColor = Convert.FromBase64String(Ic["AnnotationTitle"].ToString());
                Ic = drCategoryScheme.GetChildRows("CategoryScheme_Annotation").FirstOrDefault(n => n["AnnotationType"].ToString() == "DISSEMINATION_GREY_ICON");
                if (Ic != null)
                    IconGray = Convert.FromBase64String(Ic["AnnotationTitle"].ToString());
                Ic = drCategoryScheme.GetChildRows("CategoryScheme_Annotation").FirstOrDefault(n => n["AnnotationType"].ToString() == "DISSEMINATION_HOVER_ICON");
                if (Ic != null)
                    IconHover = Convert.FromBase64String(Ic["AnnotationTitle"].ToString());
            } catch (Exception cs) { }
        }

        public Category[] CategoryList { get { return ApiBase != null && ApiBase.CategoryList != null ? ApiBase.CategoryList.Where(cl => cl.CategorySchemeID == ID).ToArray() : new Category[] { }; } }
    }
    public class Category {
        public ApiBaseURI ApiBase { get; private set; }
        public string ApiBaseID { get; private set; }
        public CategoryScheme CategoryScheme { get { return ApiBase != null && ApiBase.CategorySchemeList != null ? ApiBase.CategorySchemeList.FirstOrDefault(cs => cs.ID == CategorySchemeID) : null; } }
        public string CategorySchemeID { get; private set; }
        public string ID { get; private set; }
        public string ParentID { get; private set; }
        public string Name { get; private set; }

        public Category(ApiBaseURI _ApiBase, string _ID, DataRow drCategory) {
            ApiBase = _ApiBase;
            ApiBaseID = _ApiBase.ID;
            ID = _ID;
            try {
                if (drCategory.GetParentRow("CategoryScheme_Category") != null)
                    CategorySchemeID = drCategory.GetParentRow("CategoryScheme_Category")["id"].ToString();
                if (drCategory.Table.DataSet.Relations["Category_Category"] != null && drCategory.GetParentRow("Category_Category") != null)
                    ParentID = drCategory.GetParentRow("Category_Category")["id"].ToString();
                DataRow name = drCategory.GetChildRows("Category_Name").FirstOrDefault(n => n["lang"].ToString() == "en");
                if (name != null)
                    Name = name["Name_Text"].ToString();
            } catch (Exception c) { }
        }
    }
    public class Categorisation {
        public ApiBaseURI ApiBase { get; private set; }
        public string ApiBaseID { get; private set; }
        public string ID { get; private set; }
        public string SourceID { get; private set; }
        public string TargetID { get; private set; }
        public string TargetParentID { get; private set; }

        public Categorisation(ApiBaseURI _ApiBase, string _ID, DataRow drCategorisation) {
            ApiBase = _ApiBase;
            ApiBaseID = _ApiBase.ID;
            ID = _ID;
            try {
                DataRow S = drCategorisation.GetChildRows("Categorisation_SourceRef").FirstOrDefault();
                if (S != null)
                    SourceID = S["id"].ToString();
                DataRow T = drCategorisation.GetChildRows("Categorisation_TargetRef").FirstOrDefault();
                if (T != null) {
                    TargetID = T["id"].ToString();
                    TargetParentID = T["maintainableParentID"].ToString();
                }
            } catch (Exception c) { }
        }
    }
    public class Dataflow {
        public ApiBaseURI ApiBase { get; private set; }
        public string ApiBaseID { get; private set; }
        public string ID { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; } = null;
        public string HTML { get; private set; } = null;
        public string SDMX { get; private set; } = null;

        public Dataflow(ApiBaseURI _ApiBase, string _ID, DataRow drDataflow) {
            ApiBase = _ApiBase;
            ApiBaseID = _ApiBase.ID;
            ID = _ID;
            try {
                DataRow name = drDataflow.GetChildRows("Dataflow_Name").FirstOrDefault(n => n["lang"].ToString() == "en");
                if (name != null)
                    Name = name["Name_Text"].ToString();
                if (drDataflow.Table.DataSet.Tables.Contains("Description") && drDataflow.Table.DataSet.Tables["Description"].Columns.Contains("Description_Text")) {
                    DataRow desc = drDataflow.GetChildRows("Dataflow_Description").FirstOrDefault(d => d["lang"].ToString() == "en");
                    if (desc != null)
                        Description = desc["Description_Text"].ToString();
                }
                if (drDataflow.Table.DataSet.Tables.Contains("Annotation") && drDataflow.Table.DataSet.Tables["Annotation"].Columns.Contains("AnnotationURL")) {
                    DataRow html = drDataflow.GetChildRows("Dataflow_Annotation").FirstOrDefault(d => d["AnnotationType"].ToString() == "ESMS_HTML");
                    if (html != null)
                        HTML = html["AnnotationURL"].ToString();
                    DataRow sdmx = drDataflow.GetChildRows("Dataflow_Annotation").FirstOrDefault(d => d["AnnotationType"].ToString() == "ESMS_SDMX");
                    if (sdmx != null)
                        SDMX = sdmx["AnnotationURL"].ToString();
                }
            } catch (Exception d) { }
        }
        public void UpdateFromDS(DataSet ds) {
            foreach (DataRow Df in ds.Tables["Dataflow"].Select("id='" + ID + "'")) {
                DataRow name = Df.GetChildRows("Dataflow_Name").FirstOrDefault(n => n["lang"].ToString() == "en");
                if (name != null)
                    Name = name["Name_Text"].ToString();
                if (ds.Tables.Contains("Description") && ds.Tables["Description"].Columns.Contains("Description_Text")) {
                    DataRow desc = Df.GetChildRows("Dataflow_Description").FirstOrDefault(d => d["lang"].ToString() == "en");
                    if (desc != null)
                        Description = desc["Description_Text"].ToString();
                } else if (Description == null)
                    Description = string.Empty;
                if (ds.Tables.Contains("Annotation") && ds.Tables["Annotation"].Columns.Contains("AnnotationURL")) {
                    DataRow html = Df.GetChildRows("Dataflow_Annotation").FirstOrDefault(d => d["AnnotationType"].ToString() == "ESMS_HTML");
                    if (html != null)
                        HTML = html["AnnotationURL"].ToString();
                    else if (HTML == null)
                        HTML = string.Empty;
                    DataRow sdmx = Df.GetChildRows("Dataflow_Annotation").FirstOrDefault(d => d["AnnotationType"].ToString() == "ESMS_SDMX");
                    if (sdmx != null)
                        SDMX = sdmx["AnnotationURL"].ToString();
                    else if (SDMX == null)
                        SDMX = string.Empty;
                } else {
                    if (HTML == null)
                        HTML = string.Empty;
                    if (SDMX == null)
                        SDMX = string.Empty;
                }
            }
        }
    }
}