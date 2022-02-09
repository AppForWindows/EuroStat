using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EuroStatApp {
    public partial class DataSetForm : DevExpress.XtraEditors.XtraForm {
        public DataSet ds;
        public DataSetForm(DataSet DS) {
            InitializeComponent();

            ds = DS;

            foreach (DataTable dt in ds.Tables) {
                DevExpress.XtraTab.XtraTabPage TP = new DevExpress.XtraTab.XtraTabPage();
                TP.Name = TP.Text = dt.TableName;
                tabControl.TabPages.Add(TP);

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

            this.StartPosition = FormStartPosition.CenterScreen;
        }
        private void simpleButton1_Click(object sender, EventArgs e) {
            using (SaveFileDialog sf = new SaveFileDialog()) {
                sf.Filter = "XML Files|*.xml";
                if (sf.ShowDialog() == DialogResult.OK)
                    ds.WriteXml(sf.FileName, XmlWriteMode.IgnoreSchema);
            }
        }
        private void DataSetForm_Shown(object sender, EventArgs e) {
            foreach (DevExpress.XtraTab.XtraTabPage TP in tabControl.TabPages)
                foreach (Control C in TP.Controls)
                    try {
                        DevExpress.XtraGrid.GridControl GC = (DevExpress.XtraGrid.GridControl)C;
                        foreach (DevExpress.XtraGrid.Views.Grid.GridView GV in GC.Views) {
                            GV.OptionsView.ShowAutoFilterRow = true;
                            foreach (DevExpress.XtraGrid.Columns.GridColumn gc in GV.Columns) {
                                if (gc.ColumnType == typeof(DateTime))
                                    gc.DisplayFormat.FormatString = "dd.MM.yyyy HH:mm:ss";
                            }
                        }
                    } catch { }
        }
    }
}
