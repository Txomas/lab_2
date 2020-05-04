using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;

namespace lab_2
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            listView.Columns[0].Width = listView.Size.Width / 2;
            listView.Columns[1].Width = -2;

            Assembly Asm = Assembly.LoadFrom("CRUD_lib.dll");
            Type[] Types = Asm.GetTypes();
            foreach (Type t in Types)
            {
                if (!t.IsAbstract)
                    comboBox.Items.Add(t.Name);
            }
            comboBox.SelectedItem = comboBox.Items[0];


        }

        private void btnAdd_Click(object sender, EventArgs e)
        {

        }
    }
}
