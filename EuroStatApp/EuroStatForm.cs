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
        LoadType curLoadTyte {
            get {
                LoadType LT = LoadType.Async;
                if (Enum.TryParse(barEditItemLoadType.EditValue.ToString(), out LT))
                    return LT;
                else
                    return LoadType.Async;
            }
            set { barEditItemLoadType.EditValue = value; }
        }

        public EuroStatForm() {
            InitializeComponent();

            repItemImageComboBoxLoadType.Items.Clear();
            repItemImageComboBoxLoadType.AddEnum(typeof(LoadType));
            curLoadTyte = LoadType.Async;
            iCBE_Source.Properties.Items.AddRange(EuroStat.Dictionary.ApiBaseList.Select(t => new DevExpress.XtraEditors.Controls.ImageComboBoxItem(t.DisplayName, t)).ToArray());
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
        private void tV_CategoryScheme_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e) {
            DevExpress.XtraGrid.Views.Tile.TileView TV = (DevExpress.XtraGrid.Views.Tile.TileView)sender;
            CategoryScheme CS = TV.GetRow(e.FocusedRowHandle) as CategoryScheme;
            if (CS == null  || ApiBase == null || ApiBase.CategorisationList == null || ApiBase.CategorisationList.Count == 0) {
                tL_Category.DataSource = null;
                gC_Dataflow.DataSource = null;
                return;
            }
            tL_Category.DataSource = ApiBase.CategoryList.Where(c => c.CategorySchemeID == CS.ID).ToList();
            Categorisation[] Categorisation = ApiBase.CategorisationList.Where(c => c.TargetParentID == CS.ID).ToArray();
            List<string> IdList = Categorisation.Select(c => c.SourceID).ToList();
            gC_Dataflow.DataSource = ApiBase.DataflowList.Where(d => IdList.Contains(d.ID)).ToList();
            tL_Category.ClearSelection();
            tL_Category.ClearFocusedColumn();
        }
        private void tV_CategoryScheme_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e) {
            if (!e.IsForGroupRow && e.Column.FieldName == "ID")
                e.DisplayText = string.Format("[{0}]", e.Value);
        }
        private void tV_CategoryScheme_ItemCustomize(object sender, DevExpress.XtraGrid.Views.Tile.TileViewItemCustomizeEventArgs e) {
            DevExpress.XtraGrid.Views.Tile.TileView TV = (DevExpress.XtraGrid.Views.Tile.TileView)sender;
            e.Item.Checked = TV.FocusedRowHandle == e.RowHandle;
        }
        private void tV_CategoryScheme_CustomItemTemplate(object sender, DevExpress.XtraGrid.Views.Tile.TileViewCustomItemTemplateEventArgs e) {
            DevExpress.XtraGrid.Views.Tile.TileView TV = (DevExpress.XtraGrid.Views.Tile.TileView)sender;
            CategoryScheme CS = TV.GetRow(e.RowHandle) as CategoryScheme;
            if (CS == null || CS.IconColor == null || CS.IconColor.Length == 0) {
                //e.Template = TV.TileTemplate.
            } else if (TV.FocusedRowHandle == e.RowHandle)
                e.Template = e.Templates["HOVER"];
            else if (tL_Category.Visible)
                e.Template = e.Templates["GRAY"];
            else
                e.Template = e.Templates["COLOR"];
        }
        private void tL_Category_FocusedNodeChanged(object sender, DevExpress.XtraTreeList.FocusedNodeChangedEventArgs e) {
            if (tL_Category.Selection.Count == 0 || tL_Category.Selection[0] == null || ApiBase.CategorisationList == null || ApiBase.CategorisationList.Count == 0)
                return;
            if (tL_Category.GetDataRecordByNode(tL_Category.Selection[0]) is Category)
                try {
                    gC_Dataflow.BeginUpdate();
                    Category C = tL_Category.GetDataRecordByNode(tL_Category.Selection[0]) as Category;
                    Categorisation[] Categorisation = ApiBase.CategorisationList.Where(c => c.TargetID.StartsWith(C.ID) || c.TargetID.Contains('.' + C.ID + '.') || c.TargetID.EndsWith('.' + C.ID)).ToArray();
                    List<string> IdList = Categorisation.Select(c => c.SourceID).ToList();
                    gC_Dataflow.DataSource = ApiBase.DataflowList.Where(d => IdList.Contains(d.ID)).ToList();
                } catch { } finally { gC_Dataflow.EndUpdate(); }
        }
        private void tL_Category_VisibleChanged(object sender, EventArgs e) {
            sC_Left.Visible = tL_Category.Visible;
            if (sC_Left.Visible)
                sC_Left.BringToFront();
            tV_CategoryScheme.RefreshData();
        }

        private void tV_Dataflow_ItemDoubleClick(object sender, DevExpress.XtraGrid.Views.Tile.TileViewItemClickEventArgs e) {
            DevExpress.XtraGrid.Views.Tile.TileView TV = (DevExpress.XtraGrid.Views.Tile.TileView)sender;
            DevExpress.XtraGrid.Views.Tile.TileViewItem TVI = e.Item as DevExpress.XtraGrid.Views.Tile.TileViewItem;
            if (TV == null || TVI == null || ApiBase == null) return;
            Dataflow Df = TV.GetRow(TVI.RowHandle) as Dataflow;
            if (Df == null) return;
            tV_Dataflow_ContextButtonClick(tV_Dataflow, new DevExpress.Utils.ContextItemClickEventArgs(TV.ContextButtons[1], null, e.Item));
        }
        private void tV_Dataflow_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e) {
            if (!e.IsForGroupRow && e.Column.FieldName == "ID")
                e.DisplayText = string.Format("[{0}]", e.Value);
        }
        private void tV_Dataflow_ContextButtonClick(object sender, DevExpress.Utils.ContextItemClickEventArgs e) {
            DevExpress.XtraGrid.Views.Tile.TileView TV = (DevExpress.XtraGrid.Views.Tile.TileView)sender;
            DevExpress.XtraGrid.Views.Tile.TileViewItem TVI = e.DataItem as DevExpress.XtraGrid.Views.Tile.TileViewItem;
            if (TV == null || TVI == null || ApiBase == null) return;
            Dataflow Df = TV.GetRow(TVI.RowHandle) as Dataflow;
            if (Df == null) return;
            if (e.Item.Name.Contains("Description") || e.Item.Name.Contains("Loading")) {
                DevExpress.Utils.ToolTipControllerShowEventArgs TT = DevExpress.Utils.ToolTipController.DefaultController.CreateShowArgs();
                TT.ToolTipLocation = DevExpress.Utils.ToolTipLocation.LeftTop;
                if (Df.Description == null || Df.HTML == null || Df.SDMX == null) {
                    TT.Title = Df.Name;
                    TT.ToolTip = "is loading...";
                    TT.IconType = DevExpress.Utils.ToolTipIconType.Hand;
                } else {
                    DevExpress.Utils.SuperToolTip STT = new DevExpress.Utils.SuperToolTip();
                    DevExpress.Utils.ToolTipTitleItem TTTI = new DevExpress.Utils.ToolTipTitleItem();
                    TTTI.Text = Df.Name;
                    STT.Items.Add(TTTI);
                    DevExpress.Utils.ToolTipItem TTId = new DevExpress.Utils.ToolTipItem();
                    TTId.Text = Df.Description;
                    STT.Items.Add(TTId);
                    STT.Items.Add(new DevExpress.Utils.ToolTipSeparatorItem());
                    if (!string.IsNullOrWhiteSpace(Df.HTML)) {
                        DevExpress.Utils.ToolTipItem TTI = new DevExpress.Utils.ToolTipItem();
                        TTI.AllowHtmlText = DevExpress.Utils.DefaultBoolean.True;
                        TTI.Text = "<href=" + Df.HTML + ">html Annotation</href>";
                        STT.Items.Add(TTI);
                    }
                    if (!string.IsNullOrWhiteSpace(Df.SDMX)) {
                        DevExpress.Utils.ToolTipItem TTI = new DevExpress.Utils.ToolTipItem();
                        TTI.AllowHtmlText = DevExpress.Utils.DefaultBoolean.True;
                        TTI.Text = "<href=" + Df.SDMX + ">sdmx Annotation</href>";
                        STT.Items.Add(TTI);
                    }
                    TT.SuperTip = STT;
                }
                DevExpress.Utils.ToolTipController.DefaultController.ShowHint(TT);
            } else if (e.Item.Name.Contains("HTML")) {
                DevExpress.Utils.HyperlinkClickEventArgs HC = new DevExpress.Utils.HyperlinkClickEventArgs();
                HC.Text = "html Annotation";
                HC.Link = Df.HTML;
                DefaultController_HyperlinkClick(e.Item, HC);
            } else if (e.Item.Name.Contains("SDMX")) {
                DevExpress.Utils.HyperlinkClickEventArgs HC = new DevExpress.Utils.HyperlinkClickEventArgs();
                HC.Text = "sdmx Annotation";
                HC.Link = Df.SDMX;
                DefaultController_HyperlinkClick(e.Item, HC);
            } else {
                gC_Dataflow.Enabled = false;
                if (!e.Item.Name.Contains("Form")) {
                    xTabControl.TabPages.Clear(true);
                    lC_Dataflow.Text = "Loading: " + Df.Name;
                    lC_DataflowURI.Text = ApiBase.DataflowDataURI(Df.ID, DataflowDataDetail.empty, false);
                }

                pP_Right.Caption = Df.Name; pP_Right.Description = string.Empty;
                if (!fP_Right.IsPopupOpen) fP_Right.ShowPopup();
                ApiBase.DataflowDataBegin(Df.ID, DataflowDataDetail.empty, false,
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
                                    formDS.Text = ApiBase.DataflowDataURI(Df.ID, DataflowDataDetail.empty, false);
                                    formDS.Height = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height - 200;
                                    formDS.Width = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width - 100;
                                    formDS.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
                                    formDS.Show(this);
                                } else
                                    try {
                                        lC_Dataflow.Text = Df.Name;
                                        lC_DataflowURI.Text = ApiBase.DataflowDataURI(Df.ID, DataflowDataDetail.empty, false);
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
            Dataflow Df = TV.GetRow(e.RowHandle) as Dataflow;
            if (Df == null) return;
            if (e.Item.Name.Contains("Loading"))
                e.Item.Visibility = Df.Description == null || Df.HTML == null || Df.SDMX == null ? DevExpress.Utils.ContextItemVisibility.Visible : DevExpress.Utils.ContextItemVisibility.Hidden;
            else if (e.Item.Name.Contains("Description"))
                e.Item.Visibility = Df.Description != null || Df.HTML != null || Df.SDMX != null ? DevExpress.Utils.ContextItemVisibility.Auto : DevExpress.Utils.ContextItemVisibility.Hidden;
            else if (e.Item.Name.Contains("HTML"))
                e.Item.Visibility = Df.HTML != null && !string.IsNullOrEmpty(Df.HTML) ? DevExpress.Utils.ContextItemVisibility.Auto : DevExpress.Utils.ContextItemVisibility.Hidden;
            else if (e.Item.Name.Contains("SDMX"))
                e.Item.Visibility = Df.SDMX != null && !string.IsNullOrEmpty(Df.SDMX) ? DevExpress.Utils.ContextItemVisibility.Auto : DevExpress.Utils.ContextItemVisibility.Hidden;
        }

        bool SetEnabled(bool En) {
            bool Res = ApiBase != null && ApiBase.CategorySchemeList != null &&  ApiBase.CategorisationList != null &&  ApiBase.DataflowList != null || En;
            gC_CategoryScheme.Enabled = iCBE_Source.Enabled = Res;
            return Res;
        }
        private async void iCBE_Source_EditValueChanged(object sender, EventArgs e) {
            if (iCBE_Source.EditValue is ApiBaseURI)
                try {
                    ApiBase = (ApiBaseURI)iCBE_Source.EditValue;
                    if (ApiBase == null) return;
                    iCBE_Source.Enabled = false;
                    if (SetEnabled(false)) {
                        SetDataSourceCategoryScheme(false);
                        SetDataSourceCategorysation(false);
                        SetDataSourceDataflow(false);
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
                                this.Invoke((MethodInvoker)delegate {
                                    SetDataSourceCategoryScheme(true);
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
                                    SetDataSourceCategorysation(true);
                                    if (fP_Center.IsPopupOpen) fP_Center.HidePopup();
                                    SetEnabled(false);
                                });
                            });

                        pP_Right.Caption = "Загрузка Потоков Данных"; pP_Right.Description = string.Empty;
                        if (!fP_Right.IsPopupOpen) fP_Right.ShowPopup();
                        ApiBase.DataflowListBegin(MetaDataListResource.dataflow, details.allstubs, false,
                            delegate (int PP, long BR, long TBR) {
                                this.Invoke((MethodInvoker)delegate {
                                    pBC_Right.EditValue = PP;
                                    pP_Right.Description = BR.ToString() + " b";
                                    if (!fP_Right.IsPopupOpen) fP_Right.ShowPopup();
                                });
                            },
                            delegate (DataSet ds, bool C, Exception E) {
                                this.Invoke((MethodInvoker)delegate {
                                    SetDataSourceDataflow(true);
                                    if (fP_Right.IsPopupOpen) fP_Right.HidePopup();
                                    SetEnabled(false);
                                });
                                ApiBase.DataflowUpdateBegin(delegate (Dataflow Df) {
                                    this.Invoke((MethodInvoker)delegate {
                                        if (ApiBase.DataflowList != null)
                                            try {
                                                tV_Dataflow.BeginUpdate();
                                                tV_Dataflow.RefreshRow(tV_Dataflow.GetRowHandle(ApiBase.DataflowList.IndexOf(Df)));
                                            } catch { } finally { tV_Dataflow.EndUpdate(); }
                                    });
                                }, delegate (DataSet dataset, bool cancel, Exception exeption) {
                                    this.Invoke((MethodInvoker)delegate {
                                        try {
                                            gC_Dataflow.BeginUpdate();
                                        } finally { gC_Dataflow.EndUpdate(); }
                                    });
                                });

                            });
                    } else if (curLoadTyte == LoadType.Async)
                        try {
                            ShowLoadMessage(this, "Загрузка Данных", "Ожидайте...");
                            Task<DataSet> CategorySchemeTask = ApiBase.CategoryListAsync(CategoryResource.categoryscheme);
                            Task<DataSet> CategoriSationTask = ApiBase.CategoryListAsync(CategoryResource.categorisation);
                            Task<DataSet> DataflowListTask = ApiBase.DataflowListAsync(MetaDataListResource.dataflow, details.allstubs, false);

                            DevExpress.XtraSplashScreen.SplashScreenManager.Default.SetWaitFormDescription("CategoryScheme");
                            DataSet CategoryScheme = await CategorySchemeTask;
                            this.Invoke((MethodInvoker)delegate {
                                SetDataSourceCategoryScheme(true);
                                if (fP_Left.IsPopupOpen) fP_Left.HidePopup();
                                SetEnabled(false);
                            });
                            DevExpress.XtraSplashScreen.SplashScreenManager.Default.SetWaitFormDescription("CategoryScheme Done");
                            DevExpress.XtraSplashScreen.SplashScreenManager.Default.SetWaitFormDescription("CategoriSation");
                            DataSet CategoriSation = await CategoriSationTask;
                            this.Invoke((MethodInvoker)delegate {
                                SetDataSourceCategorysation(true);
                                if (fP_Center.IsPopupOpen) fP_Center.HidePopup();
                                SetEnabled(false);
                            });
                            DevExpress.XtraSplashScreen.SplashScreenManager.Default.SetWaitFormDescription("CategoriSation Done");
                            DevExpress.XtraSplashScreen.SplashScreenManager.Default.SetWaitFormDescription("DataflowList");
                            DataSet DataflowList = await DataflowListTask;
                            this.Invoke((MethodInvoker)delegate {
                                SetDataSourceDataflow(true);
                                if (fP_Right.IsPopupOpen) fP_Right.HidePopup();
                                SetEnabled(false);
                            });
                            DevExpress.XtraSplashScreen.SplashScreenManager.Default.SetWaitFormDescription("DataflowList Done");
                            ApiBase.DataflowUpdateAsync(delegate (Dataflow Df) {
                                this.Invoke((MethodInvoker)delegate {
                                    if (ApiBase.DataflowList != null)
                                        try {
                                            tV_Dataflow.BeginUpdate();
                                            tV_Dataflow.RefreshRow(tV_Dataflow.GetRowHandle(ApiBase.DataflowList.IndexOf(Df)));
                                        } catch { } finally { tV_Dataflow.EndUpdate(); }
                                });
                            });
                            SetEnabled(false);
                        } catch (Exception al) { SetEnabled(true); throw al; } finally { CloseLoadMessage(); }
                    else if (curLoadTyte == LoadType.CurThread)
                        System.Threading.Tasks.Parallel.Invoke(
                            () => {
                                ApiBase.CategoryListGet(CategoryResource.categoryscheme, null);
                                this.Invoke((MethodInvoker)delegate {
                                    SetDataSourceCategoryScheme(true);
                                    if (fP_Left.IsPopupOpen) fP_Left.HidePopup();
                                    SetEnabled(false);
                                });
                            },
                            () => {
                                ApiBase.CategoryListGet(CategoryResource.categorisation, null);
                                this.Invoke((MethodInvoker)delegate {
                                    SetDataSourceCategorysation(true);
                                    if (fP_Center.IsPopupOpen) fP_Center.HidePopup();
                                    SetEnabled(false);
                                });
                            },
                            () => {
                                ApiBase.DataflowListGet(MetaDataListResource.dataflow, details.allstubs, false, null);
                                this.Invoke((MethodInvoker)delegate {
                                    SetDataSourceDataflow(true);
                                    if (fP_Right.IsPopupOpen) fP_Right.HidePopup();
                                    SetEnabled(false);
                                });
                                ApiBase.DataflowUpdateBegin(null, delegate (DataSet dataset, bool cancel, Exception exeption) {
                                    this.Invoke((MethodInvoker)delegate {
                                        try {
                                            gC_Dataflow.BeginUpdate();
                                        } finally { gC_Dataflow.EndUpdate(); }
                                    });
                                });
                            }
                        );
                } finally { gC_CategoryScheme_MouseEnter(gC_CategoryScheme, new EventArgs()); }
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
        void SetDataSourceCategoryScheme(bool Save) {
            try {
                gC_CategoryScheme.BeginUpdate(); tL_Category.BeginUpdate();
                gC_CategoryScheme.DataSource = ApiBase.CategorySchemeList;
                gC_CategoryScheme.DataMember = null;
                tL_Category.DataSource = null;
                tL_Category.DataMember = null;
                tL_Category.KeyFieldName = "ID";
                tL_Category.ParentFieldName = "ParentID";
            } catch (Exception e) { } finally { gC_CategoryScheme.EndUpdate(); tL_Category.EndUpdate(); }
            if (ApiBase == null || !Save) return;
            using (DataContext context = new DataContext())
                try {
                    if (context.ApiBaseURIes.Any(db => db.ID == ApiBase.ID))
                        context.Update(ApiBase);
                    else
                        context.ApiBaseURIes.Add(ApiBase);
                    context.SaveChanges();
                    if (ApiBase.CategorySchemeList != null)
                        foreach (CategoryScheme c in ApiBase.CategorySchemeList)
                            if (context.CategorySchemes.Any(db => db.ID == c.ID))
                                context.Update(c);
                            else
                                context.CategorySchemes.Add(c);
                    if (ApiBase.CategoryList != null)
                        foreach (Category c in ApiBase.CategoryList)
                            if (context.Categories.Any(db => db.ID == c.ID))
                                context.Update(c);
                            else
                                context.Categories.Add(c);
                    context.SaveChangesAsync();
                } catch (Exception e) { } finally { }
        }
        void SetDataSourceCategorysation(bool Save) {
            try {

            } catch { } finally { }
            if (ApiBase == null || !Save) return;
            using (DataContext context = new DataContext())
                try {
                    if (context.ApiBaseURIes.Any(db => db.ID == ApiBase.ID))
                        context.Update(ApiBase);
                    else
                        context.ApiBaseURIes.Add(ApiBase);
                    context.SaveChanges();
                    if (ApiBase.CategorisationList != null)
                        foreach (Categorisation c in ApiBase.CategorisationList)
                            if (context.Categorisations.Any(db => db.ID == c.ID))
                                context.Update(c);
                            else
                                context.Categorisations.Add(c);
                    context.SaveChangesAsync();
                } catch (Exception e) { } finally { }
        }
        void SetDataSourceDataflow(bool Save) {
            try {
                gC_Dataflow.BeginUpdate();
                gC_Dataflow.DataSource = null;
                gC_Dataflow.DataMember = null;
            } catch (Exception e) { } finally { gC_Dataflow.EndUpdate(); }
            if (ApiBase == null || !Save) return;
            using (DataContext context = new DataContext())
                try {
                    if (context.ApiBaseURIes.Any(db => db.ID == ApiBase.ID))
                        context.Update(ApiBase);
                    else
                        context.ApiBaseURIes.Add(ApiBase);
                    context.SaveChanges();
                    if (ApiBase.DataflowList != null)
                        foreach (Dataflow d in ApiBase.DataflowList)
                            if (context.Dataflows.Any(db => db.ID == d.ID))
                                context.Update(d);
                            else
                                context.Dataflows.Add(d);
                    context.SaveChangesAsync();
                } catch (Exception e) { } finally { }
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
