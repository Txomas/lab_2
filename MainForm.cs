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
using CRUD_lib;
using Serialization_lib;
using System.IO;

namespace lab_2
{
    public partial class MainForm : Form
    {
         public MainForm()
        {
            InitializeComponent();

            Assembly SerializationAsm = Assembly.LoadFrom("Serialization_lib.dll");
            List<ControlInfoAttribute> Attributes = new List<ControlInfoAttribute>();
            foreach (Type t in SerializationAsm.GetTypes())
            {
               object[] attrs = t.GetCustomAttributes(typeof(ControlInfoAttribute), false);
               if (attrs.Length > 0)
                {
                    Attributes.Add((ControlInfoAttribute)attrs[0]);
                }
            }
                        
            comboBoxType.DataSource = Attributes;
            comboBoxType.DisplayMember = "Text";
            comboBoxType.ValueMember = "type";

            Assembly CRUDAsm = Assembly.LoadFrom("CRUD_lib.dll");
            List<Type> Types = new List<Type>();
            foreach (Type t in CRUDAsm.GetTypes())
            {
                if (!t.IsAbstract && !t.IsSubclassOf(typeof(Attribute)))
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
                if (((InfoCRUD)T.Tag).type == ((InfoCRUD)treeView.SelectedNode.Tag).type) 
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
                InfoCRUD I = new InfoCRUD { type = f.FieldType };
                object[] attrs = f.GetCustomAttributes(typeof(NormalNameAttribute), false);
                I.Name = attrs.Length > 0 ? ((NormalNameAttribute)attrs[0]).Name : f.Name;
                treeView.Nodes[0].LastNode.Nodes.Add(new TreeNode(I.Name + ": "));
                treeView.Nodes[0].LastNode.LastNode.Tag = I;    
            }
            InfoCRUD i = new InfoCRUD { type = (Type)comboBox.SelectedItem };
            treeView.Nodes[0].LastNode.Tag = i;

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
                    if (P.Text == ((InfoCRUD)P.Tag).Name + ": " + treeView.SelectedNode.Text) //Is property-node value equals object-node text
                    {
                        P.Text = ((InfoCRUD)P.Tag).Name; //Clear property value
                    }
                }
            }
            
            treeView.Nodes.Remove(treeView.SelectedNode);
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (textBoxValue.Visible == true)
            {
                treeView.SelectedNode.Text = ((InfoCRUD)treeView.SelectedNode.Tag).Name + ": " + textBoxValue.Text;
                textBoxValue.Clear();
            }
            else
            {
                if (numericValue.Visible == true)
                {
                    treeView.SelectedNode.Text = ((InfoCRUD)treeView.SelectedNode.Tag).Name + ": " + numericValue.Value;
                    numericValue.Value = 0;
                }
                else
                {
                    treeView.SelectedNode.Text = ((InfoCRUD)treeView.SelectedNode.Tag).Name + ": " + comboBoxValue.Text;
                }
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
                Type fType = ((InfoCRUD)treeView.SelectedNode.Tag).type;

                if (fType == typeof(string))
                {
                    textBoxValue.Visible = true;
                    textBoxValue.Enabled = true;
                    comboBoxValue.Visible = false;
                    numericValue.Visible = false;
                }
                else
                {
                    if (fType == typeof(int))
                    {
                        numericValue.Visible = true;
                        numericValue.Enabled = true;
                        textBoxValue.Visible = false;
                        comboBoxValue.Visible = false;
                    }
                    else
                    {
                        textBoxValue.Visible = false;
                        numericValue.Visible = false;
                        comboBoxValue.Visible = true;
                        comboBoxValue.Enabled = true;

                        if (fType.IsEnum)
                        {
                            comboBoxValue.Items.Clear();
                            comboBoxValue.Items.AddRange(Enum.GetNames(fType));
                        }
                        else
                        {
                            comboBoxValue_Fill();
                        }
                    }
                }
            }
            else
            {
                btnEdit.Enabled = false;
                numericValue.Enabled = false;
                comboBoxValue.Enabled = false;
                textBoxValue.Enabled = false;
            }

            btnDelete.Enabled = treeView.SelectedNode.Level == 1 ? true : false;
        }

        private object[] GetUserCreatedObjs()
        {
            object[] objects = new object[treeView.Nodes[0].Nodes.Count];
            foreach (TreeNode T in treeView.Nodes[0].Nodes) //Look through objects
            {
                
                foreach (TreeNode P in T.Nodes) //Look through properties
                {
                    
                }
            }
            return objects;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (saveFileDialog.ShowDialog() == DialogResult.Cancel)
                return;

            ISerializer serializer = (ISerializer)Activator.CreateInstance((Type)comboBoxType.SelectedValue);
            

            using (FileStream fs = new FileStream(saveFileDialog.FileName, FileMode.OpenOrCreate))
            {
                //serializer.Serialize(objects, fs);
            }
        }

        private void numericValue_ValueChanged(object sender, EventArgs e)
        {

        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            
        }
    }
}
