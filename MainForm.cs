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

            Assembly Asm = Assembly.LoadFrom("CRUD_lib.dll");
            List<Type> Types = new List<Type>();
            foreach (Type t in Asm.GetTypes())
            {
                if (!t.IsAbstract)
                    Types.Add(t);
            }
            comboBox.DataSource = Types;
            comboBox.DisplayMember = "Name";
            comboBox.SelectedItem = comboBox.Items[0];
        }

        private void comboBoxValue_Fill()
        {
            comboBoxValue.Items.Clear();
            foreach (TreeNode T in treeView.Nodes[0].Nodes)
            {
                if ((Type)T.Tag == ((FieldInfo)treeView.SelectedNode.Tag).FieldType) 
                {
                    comboBoxValue.Items.Add(T.Text);
                }
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            treeView.Nodes[0].Nodes.Add(new TreeNode(textBoxName.Text));
            foreach (FieldInfo f in ((Type)comboBox.SelectedItem).GetFields())
            {
                treeView.Nodes[0].LastNode.Nodes.Add(new TreeNode(f.Name + ": "));
                treeView.Nodes[0].LastNode.LastNode.Tag = f;
                treeView.Nodes[0].LastNode.Tag = comboBox.SelectedItem;
            }

            treeView.Nodes[0].ExpandAll();
            if (comboBoxValue.Enabled == true)
            {
                comboBoxValue_Fill();
            }
            textBoxName.Clear();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            foreach (TreeNode T in treeView.Nodes[0].Nodes) //Look through objects
            {
                foreach (TreeNode P in T.Nodes) //Look through properties
                {
                    if (P.Text == ((FieldInfo)P.Tag).Name + ": " + treeView.SelectedNode.Text) //Is property-node value equals object-node text
                    {
                        P.Text = ((FieldInfo)P.Tag).Name; //Clear property value
                    }
                }
            }
            
            treeView.Nodes.Remove(treeView.SelectedNode);
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (textBoxValue.Visible == true)
            {
                treeView.SelectedNode.Text = ((FieldInfo)treeView.SelectedNode.Tag).Name + ": " + textBoxValue.Text;
                textBoxValue.Clear();
            }
            else
            {
                treeView.SelectedNode.Text = ((FieldInfo)treeView.SelectedNode.Tag).Name + ": " + comboBoxValue.Text;
            }
        }

        private void textBoxName_TextChanged(object sender, EventArgs e)
        {
            if (textBoxName.Text == "")
            {
                btnAdd.Enabled = false;
            }
            else
            {
                btnAdd.Enabled = true;
            }
        }

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (treeView.SelectedNode.Level == 2) //Is property selected
            {
                btnEdit.Enabled = true;
                if (((FieldInfo)treeView.SelectedNode.Tag).FieldType.Namespace == "CRUD_lib") //Is class-property selected
                {
                    textBoxValue.Visible = false;
                    comboBoxValue.Visible = true;
                    comboBoxValue.Enabled = true;
                    comboBoxValue_Fill();
                }
                else
                {
                    textBoxValue.Visible = true;
                    textBoxValue.Enabled = true;
                    comboBoxValue.Visible = false;
                }
            }
            else
            {
                btnEdit.Enabled = false;
                comboBoxValue.Enabled = false;
                textBoxValue.Enabled = false;
            }

            btnDelete.Enabled = treeView.SelectedNode.Level == 1 ? true : false;
        }
    }
}
