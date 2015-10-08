using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LiteDB;
namespace LiteDB_Management_Studio
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.buttonLoad.Enabled = false;
            comboBox1.SelectedValueChanged += ComboBox1_SelectedValueChanged;
            this.FormClosing += Form1_FormClosing;
            LiteDatabase db = new LiteDatabase(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.bin"));
            var x = db.GetCollection<Histories>(nameof(Histories));
            if (x.Count() > 0)
            {
                var item = x.FindOne(Query.All());
                this.comboBox1.Items.AddRange(item.Value);
                if (item.Value.Any()) {
                    this.comboBox1.SelectedItem = item.Value[0];
                }
                this.button2.Enabled = item.Value.Any();
            }
        }

        private void ComboBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            this.buttonLoad.Enabled = this.comboBox1.Items.Count > 0;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //saveHistory();
        }

        public class Histories
        {
            public string[] Value { get; set; }
        }

        void LoadDB(string fileName) {
            LiteDatabase db = new LiteDatabase(fileName);
            var cols = db.GetCollectionNames();
            Text = fileName;
            tabControl1.TabPages.Clear();
            foreach (var colName in cols)
            {
                var tabPage = new TabPage(colName);
                var tb = new DataTable();
                foreach (var doc in db.GetCollection(colName).FindAll())
                {
                    if (tb.Columns.Count == 0)
                        tb.Columns.AddRange(doc.Keys.Select(key => new DataColumn(key) { Caption = key }).ToArray());
                    var row = tb.NewRow();
                    foreach (var item in doc.Keys)
                    {
                        row[item] = doc[item];
                    }
                    tb.Rows.Add(row);
                }
                var dg = new DataGridView();
                dg.DataSource = tb;
                dg.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dg.Dock = DockStyle.Fill;
                tabPage.Controls.Add(dg);
                tabControl1.TabPages.Add(tabPage);
            }
            foreach (var item in comboBox1.Items)
            {
                if (item.ToString() == fileName)
                    return;
            }
            comboBox1.Items.Add(fileName);
            saveHistory();
        }



        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog f = new OpenFileDialog();
            var x = f.ShowDialog(this);
            if (x == DialogResult.OK)
            {
                try
                {
                    LoadDB(f.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.StackTrace, ex.Message);
                }
            }
        }

        private void saveHistory()
        {
            LiteDatabase db = new LiteDatabase(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.bin"));
            var x = db.GetCollection<Histories>(nameof(Histories));
            x.Delete(Query.All());
            var items = new List<string>();
            foreach (var item in comboBox1.Items)
            {
                items.Add(item.ToString());
            }
            x.Insert(new Histories()
            {
                Value = items.ToArray()
            });
        }

        private void buttonLoad_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem != null) {
                LoadDB(comboBox1.SelectedItem.ToString());
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            LiteDatabase db = new LiteDatabase(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.bin"));
            var x = db.GetCollection<Histories>(nameof(Histories));
            x.Delete(Query.All());
            comboBox1.Items.Clear();
            this.buttonLoad.Enabled = false;
        }
    }
}
