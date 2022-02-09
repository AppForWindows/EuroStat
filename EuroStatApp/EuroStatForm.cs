using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using EuroStat;

namespace EuroStatApp {
    public partial class EuroStatForm : DevExpress.XtraEditors.XtraForm {
        ApiBaseURI ApiBase;
        DataSet ds_CategoryScheme;
        DataSet ds_Categorysation;
        DataSet ds_Dataflow;
        public EuroStatForm() {
            InitializeComponent();

            iCBE_Source.Properties.Items.AddRange(EuroStat.Dictionary.ApiBaseURITypes.Select(t => new DevExpress.XtraEditors.Controls.ImageComboBoxItem(t.DisplayName(), t)).ToArray());
        }
        private void EuroStatForm_Shown(object sender, EventArgs e) {
            tL_Category.Width = pC_Left.Width;
            pC_Left_SizeChanged(pC_Left, new EventArgs());
            lC_Dataflow.Text = lC_DataflowURI.Text = string.Empty;
        }

        private void pC_Left_SizeChanged(object sender, EventArgs e) {
            gC_CategoryScheme.Dock = DockStyle.Fill;
            if (tL_Category.Width > pC_Left.Width - 100)
                tL_Category.Width = pC_Left.Width - 100;
            tL_Category.BringToFront();
            sC_Left.BringToFront();
        }
        private void gC_CategoryScheme_MouseEnter(object sender, EventArgs e) {
            tL_Category.Visible = sC_Left.Visible =  false;
            gC_CategoryScheme.BringToFront();
        }
        private void gC_CategoryScheme_MouseLeave(object sender, EventArgs e) {
            tL_Category.BringToFront();
            sC_Left.BringToFront();
            tL_Category.Visible = sC_Left.Visible =  true;
        }
        private void tV_CategoryScheme_ItemClick(object sender, DevExpress.XtraGrid.Views.Tile.TileViewItemClickEventArgs e) {
            gC_CategoryScheme_MouseLeave(gC_CategoryScheme, new EventArgs());
            DevExpress.XtraGrid.Views.Tile.TileView TV = (DevExpress.XtraGrid.Views.Tile.TileView)sender;
            DataRow CR = TV.GetDataRow(e.Item.RowHandle);
            if (CR == null)
                return;
        }
        private void tV_CategoryScheme_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e) {
            if (!e.IsForGroupRow && e.Column.FieldName == "id")
                e.DisplayText = string.Format("[{0}]", e.Value);
        }
        private void tL_Category_VisibleChanged(object sender, EventArgs e) {
            sC_Left.Visible = tL_Category.Visible;
            if (sC_Left.Visible)
                sC_Left.BringToFront();
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
            if (e.Item.Name.Contains("Description"))
                DevExpress.Utils.ToolTipController.DefaultController.ShowHint(D["DataflowDescription"].ToString(), DevExpress.Utils.ToolTipLocation.LeftTop);
            else {
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
                            if (ds == null || ds.Tables == null || ds.Tables.Count == 0) return;
                            if (e.Item.Name.Contains("Form")) {
                                DataSetForm formDS = new DataSetForm(ds);
                                formDS.Text = ApiBase.DataflowURI(D["id"].ToString(), DataflowResource.dataflow, "", DataflowReferences.none, false);
                                formDS.Height = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height - 200;
                                formDS.Width = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width - 100;
                                formDS.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
                                formDS.Show();
                            } else
                                try {
                                    lC_Dataflow.Text = D["DataflowName"].ToString();
                                    lC_DataflowURI.Text = ApiBase.DataflowURI(D["id"].ToString(), DataflowResource.dataflow, "", DataflowReferences.none, false);
                                    xTabControl.BeginUpdate();
                                    xTabControl.TabPages.Clear(true);
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
                            if (fP_Right.IsPopupOpen) fP_Right.HidePopup();
                        });
                    });
            }
        }

        private void iCBE_Source_EditValueChanged(object sender, EventArgs e) {
            if (iCBE_Source.EditValue is Type && ((Type)iCBE_Source.EditValue).IsSubclassOf(typeof(ApiBaseURI))) {
                ApiBase = ((Type)iCBE_Source.EditValue).GetConstructor(new Type[] { }).Invoke(new object[] { }) as ApiBaseURI;
                if (ApiBase == null) return;

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
                        ds_CategoryScheme = ds;
                        this.Invoke((MethodInvoker)delegate {
                            try {
                                gC_CategoryScheme.BeginUpdate(); tL_Category.BeginUpdate();
                                gC_CategoryScheme.DataSource = ds_CategoryScheme;
                                gC_CategoryScheme.DataMember = "CategoryScheme";
                                tL_Category.DataSource = ds_CategoryScheme;
                                tL_Category.DataMember = "CategoryScheme.CategoryScheme_Category";
                                tL_Category.KeyFieldName = ds_CategoryScheme.Relations["Category_Category"].ParentColumns[0].ColumnName;
                                tL_Category.ParentFieldName = ds_CategoryScheme.Relations["Category_Category"].ChildColumns[0].ColumnName;
                            } catch { }
                            finally { gC_CategoryScheme.EndUpdate(); tL_Category.EndUpdate(); }
                            if (fP_Left.IsPopupOpen) fP_Left.HidePopup();
                        });
                    });

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
                        ds_Categorysation = ds;
                        this.Invoke((MethodInvoker)delegate {
                            
                            if (fP_Center.IsPopupOpen) fP_Center.HidePopup();
                        });
                    });

                if (!fP_Right.IsPopupOpen) fP_Right.ShowPopup();
                ApiBase.MetaDataListBegin(MetaDataListResource.dataflow, details.full, false,
                    delegate (int PP, long BR, long TBR) {
                        this.Invoke((MethodInvoker)delegate {
                            pBC_Right.EditValue = PP;
                            pP_Right.Description = BR.ToString() + " b";
                            if (!fP_Right.IsPopupOpen) fP_Right.ShowPopup();
                        });
                    },
                    delegate (DataSet ds, bool C, Exception E) {
                        ds_Dataflow = ds;
                        this.Invoke((MethodInvoker)delegate {
                            try {
                                gC_Dataflow.BeginUpdate();
                                bS_Dataflow.DataSource = ds_Dataflow;
                                bS_Dataflow.DataMember = "Dataflow";
                                bS_Dataflow.Filter = string.Empty;
                            } finally { gC_Dataflow.EndUpdate(); }
                            if (fP_Right.IsPopupOpen) fP_Right.HidePopup();
                        });
                    });

                //System.Threading.Tasks.Parallel.Invoke(
                //    () => {
                //        ds_CategoryScheme = V.CategoryList(CategoryResource.categoryscheme, null);
                //        this.Invoke((MethodInvoker)delegate {
                //            try {
                //                gC_CategoryScheme.BeginUpdate(); tL_Category.BeginUpdate();
                //                gC_CategoryScheme.DataSource = ds_Category;
                //                gC_CategoryScheme.DataMember = "CategoryScheme";
                //                tL_Category.DataSource = ds_Category;
                //                tL_Category.DataMember = "CategoryScheme.CategoryScheme_Category";
                //                tL_Category.KeyFieldName = ds_Category.Relations["Category_Category"].ParentColumns[0].ColumnName;
                //                tL_Category.ParentFieldName = ds_Category.Relations["Category_Category"].ChildColumns[0].ColumnName;
                //            } finally { gC_CategoryScheme.EndUpdate(); tL_Category.EndUpdate(); }
                //        });
                //    },
                //    () => {
                //        ds_Dataflow = V.MetaDataList(MetaDataListResource.dataflow, details.full, false, null);
                //        this.Invoke((MethodInvoker)delegate {
                //            try {
                //                gC_Dataflow.BeginUpdate();
                //                bS_Dataflow.DataSource = ds_Dataflow;
                //                bS_Dataflow.DataMember = "Dataflow";
                //                bS_Dataflow.Filter = string.Empty;
                //            } finally { gC_Dataflow.EndUpdate(); }
                //        });
                //    },
                //    () => { ds_Categorysation = V.CategoryList(CategoryResource.categorisation, null); }
                //);
            }
            gC_CategoryScheme.BringToFront();
        }

        private void bE_DownLoad_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e) {
            bE_DownLoad_KeyDown(bE_DownLoad, new KeyEventArgs(Keys.Enter));
        }
        private void bE_DownLoad_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyData != Keys.Enter)
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
                this.BeginInvoke((MethodInvoker)delegate () { pBC_URI.EditValue = dpcea.ProgressPercentage; lC_Download.Text = dpcea.BytesReceived.ToString()+" b "; });
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
            //System.Threading.AutoResetEvent waiter = new System.Threading.AutoResetEvent(false);
            wClient.DownloadStringAsync(uri);//, waiter);
            //waiter.WaitOne();
        }
        private void tV_CategoryScheme_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e) {
            DevExpress.XtraGrid.Views.Tile.TileView TV = (DevExpress.XtraGrid.Views.Tile.TileView)sender;
            DataRow CR = TV.GetDataRow(e.FocusedRowHandle);
            if (CR == null || ds_Categorysation == null || ds_Categorysation.Tables.Count == 0)
                return;
            DataRow[] CS = ds_Categorysation.Tables["Categorisation"].Select("TargetParentID='" + CR["id"].ToString() + "'");
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
                    DataRow[] CS = ds_Categorysation.Tables["Categorisation"].Select(string.Format("TargetID like '{0}%' or TargetID like '%.{0}.%' or TargetID like '%.{0}'", C["id"]));
                    bS_Dataflow.Filter = "id in ('" + string.Join("','", CS.Select(r => r["SourceID"].ToString())) + "')";
                } catch { }
        }
    }
}
