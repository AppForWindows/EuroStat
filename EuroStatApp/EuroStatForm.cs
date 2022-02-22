using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using DevExpress.XtraEditors;
using DevExpress.XtraBars.Helpers;
using EuroStat;

namespace EuroStatApp {
    public partial class EuroStatForm : DevExpress.XtraBars.ToolbarForm.ToolbarForm {
        ApiBaseURI ApiBase;
        Dictionary<Type, ApiBaseURI> ApiBaseList = new Dictionary<Type, ApiBaseURI>();
        DataSet ds_CategoryScheme { get { return ApiBase != null ? ApiBase.ds_CategoryScheme : null; } }
        DataSet ds_Categorysation { get { return ApiBase != null ? ApiBase.ds_Categorysation : null; } }
        DataSet ds_Dataflow { get { return ApiBase != null ? ApiBase.ds_Dataflow : null; } }
        LoadType curLoadTyte {
            get {
                LoadType LT = LoadType.Delegate;
                if (Enum.TryParse(barEditItemLoadType.EditValue.ToString(), out LT))
                    return LT;
                else
                    return LoadType.Delegate;
            }
            set { barEditItemLoadType.EditValue = value; }
        }

        public EuroStatForm() {
            InitializeComponent();

            repItemImageComboBoxLoadType.Items.Clear();
            repItemImageComboBoxLoadType.AddEnum(typeof(LoadType));
            curLoadTyte = LoadType.Delegate;
            iCBE_Source.Properties.Items.AddRange(EuroStat.Dictionary.ApiBaseURITypes.Select(t => new DevExpress.XtraEditors.Controls.ImageComboBoxItem(t.DisplayName(), t)).ToArray());
            DevExpress.LookAndFeel.UserLookAndFeel.Default.StyleChanged += Default_StyleChanged;
            DevExpress.Utils.ToolTipController.DefaultController.KeepWhileHovered = true;
            DevExpress.Utils.ToolTipController.DefaultController.HyperlinkClick += DefaultController_HyperlinkClick;
        }

        private void DefaultController_HyperlinkClick(object sender, DevExpress.Utils.HyperlinkClickEventArgs e) {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.FileName = !string.IsNullOrWhiteSpace(e.Link) ? e.Link : e.Text;
            process.StartInfo.Verb = "open";
            process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
            try {
                process.Start();
            } catch { }
        }

        private void Default_StyleChanged(object sender, EventArgs e) {
            tL_Category.Width = pC_Left.Width;
            pC_Left_SizeChanged(pC_Left, new EventArgs());
        }

        private void EuroStatForm_Shown(object sender, EventArgs e) {
            tL_Category.Width = pC_Left.Width;
            pC_Left_SizeChanged(pC_Left, new EventArgs());
            lC_Dataflow.Text = lC_DataflowURI.Text = string.Empty;
        }

        private void pC_Left_SizeChanged(object sender, EventArgs e) {
            int sW = 4;
            try {
                DevExpress.Skins.SkinElement Sp = DevExpress.Skins.SkinManager.GetSkinElement(DevExpress.Skins.SkinProductId.Common, DevExpress.LookAndFeel.UserLookAndFeel.Default, "Splitter");
                DevExpress.Skins.SkinElement Sh = DevExpress.Skins.SkinManager.GetSkinElement(DevExpress.Skins.SkinProductId.Common, DevExpress.LookAndFeel.UserLookAndFeel.Default, "SplitterHorz");
                sW = Math.Max(Math.Max(Sp.Size.MinSize.Width, Sp.Size.MinSize.Height), Math.Max(Sh.Size.MinSize.Width, Sh.Size.MinSize.Height));
            } catch { }
            gC_CategoryScheme.SendToBack();
            gC_CategoryScheme.Dock = DockStyle.Fill;
            gC_CategoryScheme.SendToBack();
            if (tL_Category.Width > pC_Left.Width - tV_CategoryScheme.OptionsTiles.ItemSize.Height - sW - 8)
                tL_Category.Width = pC_Left.Width - tV_CategoryScheme.OptionsTiles.ItemSize.Height - sW - 8;
            tL_Category.BringToFront();
            sC_Left.BringToFront();
        }
        private void gC_CategoryScheme_MouseEnter(object sender, EventArgs e) {
            tL_Category.Visible = sC_Left.Visible = false;
            gC_CategoryScheme.BringToFront();
        }
        private void gC_CategoryScheme_MouseLeave(object sender, EventArgs e) {
            tL_Category.BringToFront();
            sC_Left.BringToFront();
            tL_Category.Visible = sC_Left.Visible = true;
        }
        private void tV_CategoryScheme_ItemClick(object sender, DevExpress.XtraGrid.Views.Tile.TileViewItemClickEventArgs e) {
            gC_CategoryScheme_MouseLeave(gC_CategoryScheme, new EventArgs());
            DevExpress.XtraGrid.Views.Tile.TileView TV = (DevExpress.XtraGrid.Views.Tile.TileView)sender;
            if (TV.FocusedRowHandle == e.Item.RowHandle)
                tV_CategoryScheme_FocusedRowChanged(sender, new DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs(TV.FocusedRowHandle, e.Item.RowHandle));
        }
        private void tV_CategoryScheme_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e) {
            if (!e.IsForGroupRow && e.Column.FieldName == "id")
                e.DisplayText = string.Format("[{0}]", e.Value);
        }
        private void tV_CategoryScheme_ItemCustomize(object sender, DevExpress.XtraGrid.Views.Tile.TileViewItemCustomizeEventArgs e) {
            DevExpress.XtraGrid.Views.Tile.TileView TV = (DevExpress.XtraGrid.Views.Tile.TileView)sender;
            e.Item.Checked = TV.FocusedRowHandle == e.RowHandle;
        }
        private void tL_Category_VisibleChanged(object sender, EventArgs e) {
            sC_Left.Visible = tL_Category.Visible;
            if (sC_Left.Visible)
                sC_Left.BringToFront();
        }
        int CategorySchemeImageIdx(string Name) {
            int idx = 9;
            if (Name.ToLower().Contains("general") || Name.ToLower().Contains("regional"))
                idx = 0;
            else if (Name.ToLower().Contains("economy") || Name.ToLower().Contains("finance"))
                idx = 1;
            else if (Name.ToLower().Contains("population") || Name.ToLower().Contains("social"))
                idx = 2;
            else if (Name.ToLower().Contains("industry") || Name.ToLower().Contains("trade") || Name.ToLower().Contains("services"))
                idx = 3;
            else if (Name.ToLower().Contains("agriculture") || Name.ToLower().Contains("forest") || Name.ToLower().Contains("fish"))
                idx = 4;
            else if (Name.ToLower().Contains("international") || Name.ToLower().Contains("goods"))
                idx = 5;
            else if (Name.ToLower().Contains("transport") || Name.ToLower().Contains("train"))
                idx = 6;
            else if (Name.ToLower().Contains("environment") || Name.ToLower().Contains("energy"))
                idx = 7;
            else if (Name.ToLower().Contains("science") || Name.ToLower().Contains("technology") || Name.ToLower().Contains("society"))
                idx = 8;
            else if (Name.ToLower().Contains("policy"))
                idx = 9;
            else if (Name.ToLower().Contains("cross") || Name.ToLower().Contains("cutting"))
                idx = 10;
            return idx;
        }
        private void tV_CategoryScheme_CustomItemTemplate(object sender, DevExpress.XtraGrid.Views.Tile.TileViewCustomItemTemplateEventArgs e) {
            DevExpress.XtraGrid.Views.Tile.TileView TV = (DevExpress.XtraGrid.Views.Tile.TileView)sender;
            DataRow CR = TV.GetDataRow(e.RowHandle);
            if (CR == null || CR["COLOR_ICON"] == DBNull.Value) {
            }else if (TV.FocusedRowHandle == e.RowHandle)
                e.Template = e.Templates["HOVER"];
            else if (tL_Category.Visible)
                e.Template = e.Templates["GREY"];
            else
                e.Template = e.Templates["COLOR"];
        }

        private void tV_Dataflow_ItemDoubleClick(object sender, DevExpress.XtraGrid.Views.Tile.TileViewItemClickEventArgs e) {
            DevExpress.XtraGrid.Views.Tile.TileView TV = (DevExpress.XtraGrid.Views.Tile.TileView)sender;
            DevExpress.XtraGrid.Views.Tile.TileViewItem TVI = e.Item as DevExpress.XtraGrid.Views.Tile.TileViewItem;
            DataRow D = TV.GetDataRow(TVI.RowHandle);
            if (TV == null || TVI == null || D == null || ApiBase == null) return;
            tV_Dataflow_ContextButtonClick(tV_Dataflow, new DevExpress.Utils.ContextItemClickEventArgs(TV.ContextButtons[1], null, e.Item));
        }
        private void tV_Dataflow_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e) {
            if (!e.IsForGroupRow && e.Column.FieldName == "id")
                e.DisplayText = string.Format("[{0}]", e.Value);
        }
        private void tV_Dataflow_ContextButtonClick(object sender, DevExpress.Utils.ContextItemClickEventArgs e) {
            DevExpress.XtraGrid.Views.Tile.TileView TV = (DevExpress.XtraGrid.Views.Tile.TileView)sender;
            DevExpress.XtraGrid.Views.Tile.TileViewItem TVI = e.DataItem as DevExpress.XtraGrid.Views.Tile.TileViewItem;
            DataRow D = TV.GetDataRow(TVI.RowHandle);
            if (TV == null || TVI == null || D == null || ApiBase == null) return;
            if (e.Item.Name.Contains("Description") || e.Item.Name.Contains("Loading")) {
                DevExpress.Utils.ToolTipControllerShowEventArgs TT = DevExpress.Utils.ToolTipController.DefaultController.CreateShowArgs();
                TT.ToolTipLocation = DevExpress.Utils.ToolTipLocation.LeftTop;
                if (D["DataflowDescription"] == DBNull.Value || D["DataflowHTML"] == DBNull.Value || D["DataflowSDMX"] == DBNull.Value) {
                    TT.Title = D["DataflowName"].ToString();
                    TT.ToolTip = "is stil loading";
                    TT.IconType = DevExpress.Utils.ToolTipIconType.Hand;
                } else {
                    DevExpress.Utils.SuperToolTip STT = new DevExpress.Utils.SuperToolTip();
                    DevExpress.Utils.ToolTipTitleItem TTTI = new DevExpress.Utils.ToolTipTitleItem();
                    TTTI.Text = D["DataflowName"].ToString();
                    STT.Items.Add(TTTI);
                    DevExpress.Utils.ToolTipItem TTId = new DevExpress.Utils.ToolTipItem();
                    TTId.Text = D["DataflowDescription"].ToString();
                    STT.Items.Add(TTId);
                    STT.Items.Add(new DevExpress.Utils.ToolTipSeparatorItem());
                    if (!string.IsNullOrWhiteSpace(D["DataflowHTML"].ToString())) {
                        DevExpress.Utils.ToolTipItem TTI = new DevExpress.Utils.ToolTipItem();
                        TTI.AllowHtmlText = DevExpress.Utils.DefaultBoolean.True;
                        TTI.Text = "<href=" + D["DataflowHTML"].ToString() + ">html Annotation</href>";
                        STT.Items.Add(TTI);
                    }
                    if (!string.IsNullOrWhiteSpace(D["DataflowSDMX"].ToString())) {
                        DevExpress.Utils.ToolTipItem TTI = new DevExpress.Utils.ToolTipItem();
                        TTI.AllowHtmlText = DevExpress.Utils.DefaultBoolean.True;
                        TTI.Text = "<href=" + D["DataflowSDMX"].ToString() + ">sdmx Annotation</href>";
                        STT.Items.Add(TTI);
                    }
                    TT.SuperTip = STT;
                }
                DevExpress.Utils.ToolTipController.DefaultController.ShowHint(TT);
            } else if (e.Item.Name.Contains("HTML")) {
                DevExpress.Utils.HyperlinkClickEventArgs HC = new DevExpress.Utils.HyperlinkClickEventArgs();
                HC.Text = "html Annotation";
                HC.Link = D["DataflowHTML"].ToString();
                DefaultController_HyperlinkClick(e.Item, HC);
            } else if (e.Item.Name.Contains("SDMX")) {
                DevExpress.Utils.HyperlinkClickEventArgs HC = new DevExpress.Utils.HyperlinkClickEventArgs();
                HC.Text = "sdmx Annotation";
                HC.Link = D["DataflowSDMX"].ToString();
                DefaultController_HyperlinkClick(e.Item, HC);
            } else {
                gC_Dataflow.Enabled = false;
                if (!e.Item.Name.Contains("Form")) {
                    xTabControl.TabPages.Clear(true);
                    lC_Dataflow.Text = "Loading: " + D["DataflowName"].ToString();
                    lC_DataflowURI.Text = ApiBase.DataflowDataURI(D["id"].ToString(), DataflowDataDetail.empty, false);
                }

                pP_Right.Caption = D["DataflowName"].ToString(); pP_Right.Description = string.Empty;
                if (!fP_Right.IsPopupOpen) fP_Right.ShowPopup();
                ApiBase.DataflowDataBegin(D["id"].ToString(), DataflowDataDetail.empty, false,
                    delegate (int PP, long BR, long TBR) {
                        this.Invoke((MethodInvoker)delegate {
                            pBC_Right.EditValue = PP;
                            pP_Right.Description = BR.ToString() + " b";
                            if (!fP_Right.IsPopupOpen) fP_Right.ShowPopup();
                        });
                    },
                    delegate (DataSet ds, bool C, Exception E) {
                        this.Invoke((MethodInvoker)delegate {
                            DataSetForm formDS = null;
                            try {
                                gC_Dataflow.Enabled = true;
                                if (TV.FocusedRowHandle != TVI.RowHandle)
                                    TV.FocusedRowHandle = TVI.RowHandle;
                                if (ds == null || ds.Tables == null || ds.Tables.Count == 0) return;
                                if (e.Item.Name.Contains("Form")) {
                                    formDS = new DataSetForm(ds);
                                    formDS.Text = ApiBase.DataflowDataURI(D["id"].ToString(), DataflowDataDetail.empty, false);
                                    formDS.Height = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height - 200;
                                    formDS.Width = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width - 100;
                                    formDS.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
                                    formDS.Show(this);
                                } else
                                    try {
                                        lC_Dataflow.Text = D["DataflowName"].ToString();
                                        lC_DataflowURI.Text = ApiBase.DataflowDataURI(D["id"].ToString(), DataflowDataDetail.empty, false);
                                        xTabControl.BeginUpdate();
                                        xTabControl.TabPages.Clear(true);
                                        if (e.Item.Name.Contains("Chart")) {

                                        } else
                                            foreach (DataTable dt in ds.Tables) {
                                                DevExpress.XtraTab.XtraTabPage TP = new DevExpress.XtraTab.XtraTabPage();
                                                TP.Name = TP.Text = dt.TableName;
                                                xTabControl.TabPages.Add(TP);

                                                DevExpress.XtraGrid.GridControl GC = new DevExpress.XtraGrid.GridControl();
                                                DevExpress.XtraGrid.Views.Grid.GridView GV = new DevExpress.XtraGrid.Views.Grid.GridView(GC);
                                                GC.MainView = GV;
                                                GV.OptionsCustomization.AllowFilter = false;
                                                GV.OptionsView.ColumnAutoWidth = false;

                                                GC.UseEmbeddedNavigator = true;
                                                GC.Dock = DockStyle.Fill;
                                                GC.Parent = TP;

                                                GC.DataSource = ds;
                                                GC.DataMember = dt.TableName;
                                            }
                                    } finally { xTabControl.EndUpdate(); }
                            } finally { if (fP_Right.IsPopupOpen) fP_Right.HidePopup(); if (formDS != null) formDS.BringToFront(); }
                        });
                    });
            }
        }
        private void tV_Dataflow_ItemCustomize(object sender, DevExpress.XtraGrid.Views.Tile.TileViewItemCustomizeEventArgs e) {

        }
        private void tV_Dataflow_ContextButtonCustomize(object sender, DevExpress.XtraGrid.Views.Tile.TileViewContextButtonCustomizeEventArgs e) {
            DevExpress.XtraGrid.Views.Tile.TileView TV = (DevExpress.XtraGrid.Views.Tile.TileView)sender;
            DataRow D = TV.GetDataRow(e.RowHandle);
            if (D == null) return;
            if (e.Item.Name.Contains("Loading"))
                e.Item.Visibility = D["DataflowDescription"] == DBNull.Value || D["DataflowHTML"] == DBNull.Value || D["DataflowSDMX"] == DBNull.Value ? DevExpress.Utils.ContextItemVisibility.Visible : DevExpress.Utils.ContextItemVisibility.Hidden;
            else if (e.Item.Name.Contains("Description"))
                e.Item.Visibility = D["DataflowDescription"] != DBNull.Value || D["DataflowHTML"] != DBNull.Value || D["DataflowSDMX"] != DBNull.Value ? DevExpress.Utils.ContextItemVisibility.Auto : DevExpress.Utils.ContextItemVisibility.Hidden;
            else if (e.Item.Name.Contains("HTML"))
                e.Item.Visibility = D["DataflowHTML"] != DBNull.Value && !string.IsNullOrEmpty(D["DataflowHTML"].ToString()) ? DevExpress.Utils.ContextItemVisibility.Auto : DevExpress.Utils.ContextItemVisibility.Hidden;
            else if (e.Item.Name.Contains("SDMX"))
                e.Item.Visibility = D["DataflowSDMX"] != DBNull.Value && !string.IsNullOrEmpty(D["DataflowSDMX"].ToString()) ? DevExpress.Utils.ContextItemVisibility.Auto : DevExpress.Utils.ContextItemVisibility.Hidden;
        }

        bool SetEnabled(bool En) {
            bool Res = ds_CategoryScheme != null && ds_Categorysation != null && ds_Dataflow != null || En;
            gC_CategoryScheme.Enabled = iCBE_Source.Enabled = Res;
            return Res;
        }
        private async void iCBE_Source_EditValueChanged(object sender, EventArgs e) {
            if (iCBE_Source.EditValue is Type && ((Type)iCBE_Source.EditValue).IsSubclassOf(typeof(ApiBaseURI)))
                try {
                    ApiBase = ApiBaseList.ContainsKey((Type)iCBE_Source.EditValue) ? ApiBaseList[(Type)iCBE_Source.EditValue] : ((Type)iCBE_Source.EditValue).GetConstructor(new Type[] { }).Invoke(new object[] { }) as ApiBaseURI;
                    if (ApiBase == null) return;
                    if (!ApiBaseList.ContainsKey((Type)iCBE_Source.EditValue))
                        ApiBaseList.Add((Type)iCBE_Source.EditValue, ApiBase);
                    if (SetEnabled(false)) {
                        SetDataSourceCategoryScheme();
                        SetDataSourceCategorysation();
                        SetDataSourceDataflow();
                        return;
                    }
                    iCBE_Source.Enabled = false;

                    if (curLoadTyte == LoadType.Delegate) {
                        pP_Left.Caption = "Загрузка Каталогов"; pP_Left.Description = string.Empty;
                        if (!fP_Left.IsPopupOpen) fP_Left.ShowPopup();
                        ApiBase.CategoryListBegin(CategoryResource.categoryscheme,
                            delegate (int PP, long BR, long TBR) {
                                this.Invoke((MethodInvoker)delegate {
                                    pBC_Left.EditValue = PP;
                                    pP_Left.Description = BR.ToString() + " b";
                                    if (!fP_Left.IsPopupOpen) fP_Left.ShowPopup();
                                });
                            },
                            delegate (DataSet ds, bool C, Exception E) {
                                AddPic();
                                this.Invoke((MethodInvoker)delegate {
                                    SetDataSourceCategoryScheme();
                                    if (fP_Left.IsPopupOpen) fP_Left.HidePopup();
                                    SetEnabled(false);
                                });
                            });

                        pP_Center.Caption = "Загрузка Связей"; pP_Center.Description = string.Empty;
                        if (!fP_Center.IsPopupOpen) fP_Center.ShowPopup();
                        ApiBase.CategoryListBegin(CategoryResource.categorisation,
                            delegate (int PP, long BR, long TBR) {
                                this.Invoke((MethodInvoker)delegate {
                                    pBC_Center.EditValue = PP;
                                    pP_Center.Description = BR.ToString() + " b";
                                    if (!fP_Center.IsPopupOpen) fP_Center.ShowPopup();
                                });
                            },
                            delegate (DataSet ds, bool C, Exception E) {
                                this.Invoke((MethodInvoker)delegate {
                                    SetDataSourceCategorysation();
                                    if (fP_Center.IsPopupOpen) fP_Center.HidePopup();
                                    SetEnabled(false);
                                });
                            });

                        pP_Right.Caption = "Загрузка Потоков Данных"; pP_Right.Description = string.Empty;
                        if (!fP_Right.IsPopupOpen) fP_Right.ShowPopup();
                        ApiBase.MetaDataListBegin(MetaDataListResource.dataflow, details.allstubs, false,
                            delegate (int PP, long BR, long TBR) {
                                this.Invoke((MethodInvoker)delegate {
                                    pBC_Right.EditValue = PP;
                                    pP_Right.Description = BR.ToString() + " b";
                                    if (!fP_Right.IsPopupOpen) fP_Right.ShowPopup();
                                });
                            },
                            delegate (DataSet ds, bool C, Exception E) {
                                this.Invoke((MethodInvoker)delegate {
                                    SetDataSourceDataflow();
                                    if (fP_Right.IsPopupOpen) fP_Right.HidePopup();
                                    SetEnabled(false);
                                });
                                ApiBase.DataflowUpdateBegin(delegate (DataRow D) {
                                    this.Invoke((MethodInvoker)delegate {
                                        try {
                                            tV_Dataflow.BeginUpdate();
                                            tV_Dataflow.RefreshRow(tV_Dataflow.GetRowHandle(ApiBase.Dataflow.Rows.IndexOf(D)));
                                        } catch { } finally { tV_Dataflow.EndUpdate(); }
                                    });
                                }, delegate (DataSet dataset, bool cancel, Exception exeption) {
                                    this.Invoke((MethodInvoker)delegate {
                                        try {
                                            gC_Dataflow.BeginUpdate();
                                            bS_Dataflow.DataSource = ds_Dataflow;
                                            bS_Dataflow.DataMember = ApiBase.Dataflow.TableName;
                                        } finally { gC_Dataflow.EndUpdate(); }
                                    });
                                });

                            });
                    } else if (curLoadTyte == LoadType.Async)
                        try {
                            ShowLoadMessage(this, "Загрузка Данных", "Ожидайте...");
                            Task<DataSet> CategorySchemeTask = ApiBase.CategoryListAsync(CategoryResource.categoryscheme);
                            Task<DataSet> CategoriSationTask = ApiBase.CategoryListAsync(CategoryResource.categorisation);
                            Task<DataSet> MetaDataListTask = ApiBase.MetaDataListAsync(MetaDataListResource.dataflow, details.allstubs, false);

                            DevExpress.XtraSplashScreen.SplashScreenManager.Default.SetWaitFormDescription("CategoryScheme");
                            DataSet CategoryScheme = await CategorySchemeTask;
                            AddPic();
                            this.Invoke((MethodInvoker)delegate {
                                SetDataSourceCategoryScheme();
                                if (fP_Left.IsPopupOpen) fP_Left.HidePopup();
                                SetEnabled(false);
                            });
                            DevExpress.XtraSplashScreen.SplashScreenManager.Default.SetWaitFormDescription("CategoryScheme Done");
                            DevExpress.XtraSplashScreen.SplashScreenManager.Default.SetWaitFormDescription("CategoriSation");
                            DataSet CategoriSation = await CategoriSationTask;
                            this.Invoke((MethodInvoker)delegate {
                                SetDataSourceCategorysation();
                                if (fP_Center.IsPopupOpen) fP_Center.HidePopup();
                                SetEnabled(false);
                            });
                            DevExpress.XtraSplashScreen.SplashScreenManager.Default.SetWaitFormDescription("CategoriSation Done");
                            DevExpress.XtraSplashScreen.SplashScreenManager.Default.SetWaitFormDescription("MetaDataList");
                            DataSet MetaDataList = await MetaDataListTask;
                            this.Invoke((MethodInvoker)delegate {
                                SetDataSourceDataflow();
                                if (fP_Right.IsPopupOpen) fP_Right.HidePopup();
                                SetEnabled(false);
                            });
                            DevExpress.XtraSplashScreen.SplashScreenManager.Default.SetWaitFormDescription("MetaDataList Done");
                            ApiBase.DataflowUpdateAsync(delegate (DataRow D) {
                                this.Invoke((MethodInvoker)delegate {
                                    try {
                                        tV_Dataflow.BeginUpdate();
                                        tV_Dataflow.RefreshRow(tV_Dataflow.GetRowHandle(ApiBase.Dataflow.Rows.IndexOf(D)));
                                    } catch { } finally { tV_Dataflow.EndUpdate(); }
                                });
                            });
                            SetEnabled(false);
                        } catch (Exception al) { SetEnabled(true); throw al; } finally { CloseLoadMessage(); }
                    else if (curLoadTyte == LoadType.CurThread)
                        System.Threading.Tasks.Parallel.Invoke(
                            () => {
                                ApiBase.ds_CategoryScheme = ApiBase.CategoryList(CategoryResource.categoryscheme, null);
                                AddPic();
                                this.Invoke((MethodInvoker)delegate {
                                    SetDataSourceCategoryScheme();
                                    if (fP_Left.IsPopupOpen) fP_Left.HidePopup();
                                    SetEnabled(false);
                                });
                            },
                            () => {
                                ApiBase.ds_Categorysation = ApiBase.CategoryList(CategoryResource.categorisation, null);
                                this.Invoke((MethodInvoker)delegate {
                                    SetDataSourceCategorysation();
                                    if (fP_Center.IsPopupOpen) fP_Center.HidePopup();
                                    SetEnabled(false);
                                });
                            },
                            () => {
                                ApiBase.ds_Dataflow = ApiBase.MetaDataList(MetaDataListResource.dataflow, details.allstubs, false, null);
                                this.Invoke((MethodInvoker)delegate {
                                    SetDataSourceDataflow();
                                    if (fP_Right.IsPopupOpen) fP_Right.HidePopup();
                                    SetEnabled(false);
                                });
                                ApiBase.DataflowUpdateBegin(null, delegate (DataSet dataset, bool cancel, Exception exeption) {
                                    this.Invoke((MethodInvoker)delegate {
                                        try {
                                            gC_Dataflow.BeginUpdate();
                                            bS_Dataflow.DataSource = ds_Dataflow;
                                            bS_Dataflow.DataMember = ApiBase.Dataflow.TableName;
                                        } finally { gC_Dataflow.EndUpdate(); }
                                    });
                                });
                            }
                        );
                } finally { gC_CategoryScheme_MouseEnter(gC_CategoryScheme, new EventArgs()); }
        }
        void AddPic() {
            if (ApiBase.CategoryScheme != null && ApiBase.CategoryScheme.Columns.Contains(ApiBase.CategorySchemeName)) {
                ApiBase.CategoryScheme.Columns.Add("PicIdx", typeof(int));
                ApiBase.CategoryScheme.Columns.Add("Pic", typeof(Image));
                foreach (DataRow CS in ApiBase.CategoryScheme.Rows) {
                    CS["PicIdx"] = CategorySchemeImageIdx(CS[ApiBase.CategorySchemeName].ToString());
                    CS["Pic"] = iC.Images[(int)CS["PicIdx"]];
                }
            }
        }
        private void iCBE_Source_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e) {
            if (e.Button.Kind == DevExpress.XtraEditors.Controls.ButtonPredefines.OK) {
                if (ApiBase != null)
                    ApiBase.ClearDataSet();
                iCBE_Source_EditValueChanged(iCBE_Source, new EventArgs());
            }
        }
        private void iCBE_Source_EnabledChanged(object sender, EventArgs e) {
            foreach (DevExpress.XtraEditors.Controls.EditorButton b in iCBE_Source.Properties.Buttons)
                b.Enabled = (sender as DevExpress.XtraEditors.ImageComboBoxEdit).Enabled;
        }
        void SetDataSourceCategoryScheme() {
            try {
                gC_CategoryScheme.BeginUpdate(); tL_Category.BeginUpdate();
                gC_CategoryScheme.DataSource = ds_CategoryScheme;
                gC_CategoryScheme.DataMember = ApiBase.CategoryScheme.TableName;
                tL_Category.DataSource = ds_CategoryScheme;
                tL_Category.DataMember = string.Format("{0}.{0}_{1}", ApiBase.CategoryScheme.TableName, ApiBase.Category.TableName);
                tL_Category.KeyFieldName = ApiBase.CategoryKeyFieldName;
                tL_Category.ParentFieldName = ApiBase.CategoryParentFieldName;
            } catch { } finally { gC_CategoryScheme.EndUpdate(); tL_Category.EndUpdate(); }
        }
        void SetDataSourceCategorysation() {
            try {

            } catch { } finally { }
        }
        void SetDataSourceDataflow() {
            try {
                gC_Dataflow.BeginUpdate();
                bS_Dataflow.DataSource = ds_Dataflow;
                bS_Dataflow.DataMember = ApiBase.Dataflow.TableName;
                bS_Dataflow.Filter = string.Empty;
            } finally { gC_Dataflow.EndUpdate(); }
        }

        private void bE_DownLoad_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e) {
            if (e.Button.Kind == DevExpress.XtraEditors.Controls.ButtonPredefines.Search)
                try {
                    System.Net.WebRequest req = System.Net.WebRequest.Create(bE_DownLoad.Text);
                    req.Method = "HEAD";
                    using (System.Net.WebResponse resp = req.GetResponse())
                        if (resp.ContentLength > 0)
                            XtraMessageBox.Show(resp.ContentLength.ToString(), bE_DownLoad.Text);
                        else if (long.TryParse(resp.Headers.Get("Content-Length"), out long ContentLength))
                            XtraMessageBox.Show(ContentLength.ToString(), bE_DownLoad.Text);
                        else
                            throw new Exception("No ContentLength, No Header Content-Length");
                } catch (Exception gfs) { XtraMessageBox.Show(gfs.Message, "GetFileSize"); }
            else
                bE_DownLoad_KeyDown(bE_DownLoad, new KeyEventArgs(Keys.Enter));
        }
        private void bE_DownLoad_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyData != Keys.Enter || string.IsNullOrWhiteSpace(bE_DownLoad.Text))
                return;
            pBC_URI.Parent = bE_DownLoad;
            pBC_URI.BringToFront();
            pBC_URI.Dock = DockStyle.Fill;
            pBC_URI.EditValue = 0;
            lC_URI.Text = bE_DownLoad.Text;
            lC_URI.Parent = pBC_URI;
            lC_URI.Dock = DockStyle.Fill;
            lC_URI.BringToFront();
            lC_URI.Visible = pBC_URI.Visible = true;

            System.Net.WebClient wClient = new System.Net.WebClient();
            Uri uri = new Uri(bE_DownLoad.Text);
            wClient.DownloadProgressChanged += delegate (object o, System.Net.DownloadProgressChangedEventArgs dpcea) {
                this.BeginInvoke((MethodInvoker)delegate () { pBC_URI.EditValue = dpcea.ProgressPercentage; lC_Download.Text = dpcea.BytesReceived.ToString() + " b "; });
            };
            wClient.DownloadStringCompleted += delegate (object o, System.Net.DownloadStringCompletedEventArgs dscea) {
                if (dscea.Cancelled) return;
                DataSet ds = new DataSet();
                using (System.IO.StringReader stringReader = new System.IO.StringReader(Components.ModifXML(dscea.Result)))
                    ds.ReadXml(stringReader);
                lC_Download.Text = "URI: ";
                lC_URI.Visible = pBC_URI.Visible = false;
                if (ds != null && ds.Tables != null && ds.Tables.Count > 0) {
                    DataSetForm formDS = new DataSetForm(ds);
                    formDS.Text = bE_DownLoad.Text;
                    formDS.Height = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height - 200;
                    formDS.Width = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width - 100;
                    formDS.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
                    formDS.Show();
                }
            };
            wClient.DownloadStringAsync(uri);
        }
        private void bE_DownLoad_TextChanged(object sender, EventArgs e) {
            bE_DownLoad.Properties.Buttons[1].Enabled = !string.IsNullOrWhiteSpace(bE_DownLoad.Text);
        }
        private void bE_DownLoad_Click(object sender, EventArgs e) {
            //bE_DownLoad.SelectAll();
        }

        private void tV_CategoryScheme_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e) {
            DevExpress.XtraGrid.Views.Tile.TileView TV = (DevExpress.XtraGrid.Views.Tile.TileView)sender;
            DataRow CR = TV.GetDataRow(e.FocusedRowHandle);
            if (CR == null || ds_Categorysation == null || ds_Categorysation.Tables.Count == 0)
                return;
            DataRow[] CS = ApiBase.Categorisation.Select("TargetParentID='" + CR["id"].ToString() + "'");
            bS_Dataflow.Filter = "id in ('" + string.Join("','", CS.Select(r => r["SourceID"].ToString())) + "')";
            tL_Category.ClearSelection();
            tL_Category.ClearFocusedColumn();
        }

        private void tL_Category_FocusedNodeChanged(object sender, DevExpress.XtraTreeList.FocusedNodeChangedEventArgs e) {
            if (tL_Category.Selection.Count == 0 || tL_Category.Selection[0] == null || ds_Categorysation == null || ds_Categorysation.Tables.Count == 0)
                return;
            if (tL_Category.GetDataRecordByNode(tL_Category.Selection[0]) is DataRowView)
                try {
                    DataRow C = ((DataRowView)tL_Category.GetDataRecordByNode(tL_Category.Selection[0])).Row;
                    DataRow[] CS = ApiBase.Categorisation.Select(string.Format("TargetID like '{0}%' or TargetID like '%.{0}.%' or TargetID like '%.{0}'", C["id"]));
                    bS_Dataflow.Filter = "id in ('" + string.Join("','", CS.Select(r => r["SourceID"].ToString())) + "')";
                } catch { }
        }

        public static void ShowLoadMessage(Form Parent, string Caption, string Description) {
            if (DevExpress.XtraSplashScreen.SplashScreenManager.Default == null)
                DevExpress.XtraSplashScreen.SplashScreenManager.ShowForm(Parent, typeof(CommonWaitForm), true, true, false, DevExpress.XtraSplashScreen.ParentFormState.Locked);
            DevExpress.XtraSplashScreen.SplashScreenManager.Default.SetWaitFormCaption(Caption);
            DevExpress.XtraSplashScreen.SplashScreenManager.Default.SetWaitFormDescription(Description);
        }
        public static void CloseLoadMessage() {
            if (DevExpress.XtraSplashScreen.SplashScreenManager.Default?.ActiveSplashFormTypeInfo != null && DevExpress.XtraSplashScreen.SplashScreenManager.Default.ActiveSplashFormTypeInfo.TypeName == typeof(CommonWaitForm).FullName) {
                if (DevExpress.XtraSplashScreen.SplashScreenManager.Default.IsSplashFormVisible)
                    DevExpress.XtraSplashScreen.SplashScreenManager.CloseForm(false, true);
            }
        }
    }
    public enum LoadType {
        [Description("Delegate")]
        Delegate,
        [Description("Async")]
        Async,
        [Description("CurThread")]
        CurThread
    }
}
