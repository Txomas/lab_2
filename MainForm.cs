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
using IArchivatorLib;

namespace lab_2
{
    public partial class MainForm : Form
    {
         public MainForm()
        {
            InitializeComponent();

            comboBoxPlugin.DataSource = PluginsManager.GetPlugins();
            comboBoxPlugin.DisplayMember = "Name";

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
                if (!t.IsAbstract && !t.IsSubclassOf(typeof(Attribute)) && !t.IsEnum)
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
                    comboBoxValue.Items.Add(((InfoCRUD)T.Nodes[0].Tag).Value);
                }
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            treeView.Nodes[0].Nodes.Add(new TreeNode(comboBox.Text));
            foreach (PropertyInfo f in ((Type)comboBox.SelectedItem).GetProperties())
            {
                InfoCRUD I = new InfoCRUD { type = f.PropertyType };
                
                object[] attrs = f.GetCustomAttributes(typeof(NormalNameAttribute), false);
                I.Name = attrs.Length > 0 ? ((NormalNameAttribute)attrs[0]).Name : f.Name;
                I.Name += ": ";

                if (I.Name == "Id: ")
                {
                    I.Value = Guid.NewGuid();
                    treeView.Nodes[0].LastNode.Nodes.Add(new TreeNode(I.Name + I.Value.ToString()));
                }
                else
                {
                    treeView.Nodes[0].LastNode.Nodes.Add(new TreeNode(I.Name));
                    I.Value = f.PropertyType == typeof(string) ? "" : Activator.CreateInstance(f.PropertyType);
                }
                treeView.Nodes[0].LastNode.LastNode.Tag = I;    
            }
            InfoCRUD i = new InfoCRUD { type = (Type)comboBox.SelectedItem };
            treeView.Nodes[0].LastNode.Tag = i;

            treeView.Nodes[0].ExpandAll();
            if (comboBoxValue.Enabled == true)
            {
                comboBoxValue_Fill();
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (((InfoCRUD)treeView.SelectedNode.Nodes[0].Tag).Name == "Id: ")
            {
                string id = ((InfoCRUD)treeView.SelectedNode.Nodes[0].Tag).Value.ToString();
                foreach (TreeNode T in treeView.Nodes[0].Nodes) //Look through objects
                {
                    foreach (TreeNode P in T.Nodes) //Look through properties
                    {
                        if (P.Text == ((InfoCRUD)P.Tag).Name + id) //Is property-node value equals object-node text
                        {
                            P.Text = ((InfoCRUD)P.Tag).Name; //Clear property value
                        }
                    }
                }
            }

            treeView.Nodes.Remove(treeView.SelectedNode);
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (textBoxValue.Visible == true)
            {
                treeView.SelectedNode.Text = ((InfoCRUD)treeView.SelectedNode.Tag).Name + textBoxValue.Text;
                ((InfoCRUD)treeView.SelectedNode.Tag).Value = textBoxValue.Text;
                textBoxValue.Clear();
            }
            else
            {
                if (numericValue.Visible == true)
                {
                    treeView.SelectedNode.Text = ((InfoCRUD)treeView.SelectedNode.Tag).Name + numericValue.Value;
                    ((InfoCRUD)treeView.SelectedNode.Tag).Value = (int)numericValue.Value;
                    numericValue.Value = 0;
                }
                else
                {
                    treeView.SelectedNode.Text = ((InfoCRUD)treeView.SelectedNode.Tag).Name + comboBoxValue.Text;
                    if (((InfoCRUD)treeView.SelectedNode.Tag).type.IsEnum)
                    {
                        ((InfoCRUD)treeView.SelectedNode.Tag).Value = Enum.Parse(((InfoCRUD)treeView.SelectedNode.Tag).type, comboBoxValue.Text);
                    }
                    else
                    {
                        ((InfoCRUD)treeView.SelectedNode.Tag).Value = comboBoxValue.SelectedItem;
                    }    

                }
            }
        }

        private void textBoxName_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if ((treeView.SelectedNode.Level == 2) && (!treeView.SelectedNode.Text.StartsWith("Id: "))) //Is property selected
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
            int i = 0;
            InfoCRUD infoCRUD;
            foreach (TreeNode T in treeView.Nodes[0].Nodes) //Look through objects
            {
                infoCRUD = (InfoCRUD)T.Tag;
                PropertyInfo[] Fields = infoCRUD.type.GetProperties();
                int j = 0;
                object obj = Activator.CreateInstance(infoCRUD.type);
                foreach (TreeNode P in T.Nodes) //Look through properties
                {
                    infoCRUD = (InfoCRUD)P.Tag;
                    if ((infoCRUD.Value is Guid) && (infoCRUD.Name != "Id: "))
                    {
                        foreach (TreeNode t in treeView.Nodes[0].Nodes) //Look through objects
                        {
                            if (((InfoCRUD)t.Nodes[0].Tag).Name == "Id: ")
                            {
                                if (((InfoCRUD)t.Nodes[0].Tag).Value == infoCRUD.Value)
                                {
                                    PropertyInfo[] fields = ((InfoCRUD)t.Tag).type.GetProperties();
                                    int k = 0;
                                    object o = Activator.CreateInstance(((InfoCRUD)t.Tag).type);
                                    foreach (TreeNode p in t.Nodes) //Look through properties
                                    {
                                        fields[k].SetValue(o, ((InfoCRUD)p.Tag).Value);
                                        k++;
                                    }
                                    infoCRUD.Value = o;
                                }
                            }   
                        }
                    }
                    Fields[j].SetValue(obj, infoCRUD.Value);
                    j++;
                }
                objects[i] = obj;
                i++;
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
                serializer.Serialize(GetUserCreatedObjs(), fs);
            }

            IArchivator archivator = (IArchivator)Activator.CreateInstance(comboBoxPlugin.SelectedValue.GetType());
            archivator.Compress(new FileInfo(saveFileDialog.FileName));
        }

        private void numericValue_ValueChanged(object sender, EventArgs e)
        {

        }

        private void SetUserCreatedObjs(object[] objs)
        {
            treeView.Nodes[0].Nodes.Clear();
            foreach (object obj in objs)
            {
                treeView.Nodes[0].Nodes.Add(new TreeNode(obj.GetType().Name));
                foreach (PropertyInfo f in obj.GetType().GetProperties())
                {
                    InfoCRUD I = new InfoCRUD { type = f.PropertyType };
                    I.Value = f.GetValue(obj);
                    object[] attrs = f.GetCustomAttributes(typeof(NormalNameAttribute), false);
                    I.Name = attrs.Length > 0 ? ((NormalNameAttribute)attrs[0]).Name : f.Name;
                    I.Name += ": ";
                    treeView.Nodes[0].LastNode.Nodes.Add(new TreeNode(I.Name + I.Value.ToString()));
                    treeView.Nodes[0].LastNode.LastNode.Tag = I;
                }
                InfoCRUD i = new InfoCRUD { type = obj.GetType() };
                treeView.Nodes[0].LastNode.Tag = i;
            }

            treeView.Nodes[0].ExpandAll();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.Cancel)
                return;

            string FileName = openFileDialog.FileName;
            if (!FileName.EndsWith(".dat"))
            {
                var plugins = PluginsManager.GetPlugins();
                bool IsPluginExist = false;
                foreach (var plugin in plugins)
                {
                    if (FileName.EndsWith(plugin.Extension))
                    {
                        IsPluginExist = true;
                        FileName = plugin.Decompress(new FileInfo(FileName));
                        break;
                    }
                }
                if (!IsPluginExist)
                {
                    MessageBox.Show("No plugin for this type of file");
                    return;
                }
            }

            ISerializer serializer = (ISerializer)Activator.CreateInstance((Type)comboBoxType.SelectedValue);
            using (FileStream fs = new FileStream(FileName, FileMode.OpenOrCreate))
            {
                SetUserCreatedObjs(serializer.Deserialize(fs));
            }
        }

        private void btnLoadPlugin_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.Cancel)
                return;
            if (openFileDialog.FileName.EndsWith(".dll"))
            {
                PluginsManager.LoadPlugin(openFileDialog.FileName);
                comboBoxPlugin.DataSource = PluginsManager.GetPlugins();
            }
            else
            {
                MessageBox.Show("Plugin file should be .dll file");
            }
        }

        private void btnDeletePlugin_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.Cancel)
                return;
            PluginsManager.DeletePlugin(openFileDialog.FileName);
            comboBoxPlugin.DataSource = PluginsManager.GetPlugins();
        }
    }
}