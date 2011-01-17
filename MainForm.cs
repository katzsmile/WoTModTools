using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml;
using Packed_Section_Reader;
using Primitive_File_Reader;

namespace wottools
{
    public partial class MainForm : Form
    {
        public string PackedFileName = "";
        public static readonly string sver = "0.5";
        public static readonly string stitle = "WoT Mod Tools ";
        public Packed_Section PS = new Packed_Section();
        public Primitive_File PF = new Primitive_File();
        
        public static readonly Int32 Binary_Header = 0x42a14e65;        

        public XmlDocument xDoc;

        public MainForm(string args)
        {
            InitializeComponent();
            if (args.Length != 0)
            {
                openFile(args);
            }
        }

        private string FormatXml(string sUnformattedXml)
        {
            //load unformatted xml into a dom

            XmlDocument xd = new XmlDocument();
            xd.LoadXml(sUnformattedXml);

            //will hold formatted xml

            StringBuilder sb = new StringBuilder();

            //pumps the formatted xml into the StringBuilder above

            StringWriter sw = new StringWriter(sb);

            //does the formatting

            XmlTextWriter xtw = null;

            try
            {
                //point the xtw at the StringWriter

                xtw = new XmlTextWriter(sw);

                //we want the output formatted

                xtw.Formatting = Formatting.Indented;

                //get the dom to dump its contents into the xtw 

                xd.WriteTo(xtw);
            }
            finally
            {
                //clean up even if error

                if (xtw != null)
                    xtw.Close();
            }

            //return the formatted xml

            return sb.ToString();
        }

        public void DecodePackedFile(BinaryReader reader)
        {            
            reader.ReadSByte();
            
            List<string> dictionary = PS.readDictionary(reader);

            XmlNode xmlroot = xDoc.CreateNode(XmlNodeType.Element, PackedFileName, "");

            PS.readElement(reader, xmlroot, xDoc, dictionary);
            
            xDoc.AppendChild(xmlroot);

            txtOut.AppendText(FormatXml(xDoc.OuterXml));            
        }     

        public void ReadPrimitiveFile(string file)
        {
            FileStream F = new FileStream(file, FileMode.Open, FileAccess.Read);
            BinaryReader reader = new BinaryReader(F);

            XmlComment ptiComment = xDoc.CreateComment("DO NOT SAVE THIS FILE! THIS CODE IS JUST FOR INFORMATION PUPORSES!");

            XmlNode xmlprimitives = xDoc.CreateNode(XmlNodeType.Element, "primitives", "");

            PF.ReadPrimitives(reader, xmlprimitives, xDoc);

            xDoc.AppendChild(ptiComment);
            xDoc.AppendChild(xmlprimitives);

            txtOut.AppendText(FormatXml(xDoc.OuterXml));  
        }

        public void openFile(string file)
        {
            saveAsToolStripMenuItem.Enabled = false;
            btnSave.Enabled = false;
            xDoc = new XmlDocument();
            txtOut.Clear();
            PackedFileName = Path.GetFileName(file);
            PackedFileName = PackedFileName.ToLower();
            Text = stitle + sver + " - " + PackedFileName;
            FileStream F = new FileStream(file, FileMode.Open, FileAccess.Read);
            BinaryReader reader = new BinaryReader(F);
            Int32 head = reader.ReadInt32();
            if (head == Packed_Section.Packed_Header)
            {
                DecodePackedFile(reader);
                saveAsToolStripMenuItem.Enabled = true;
                btnSave.Enabled = true;
            }
            else if (head == Binary_Header)
            {
                ReadPrimitiveFile(file);
                //saveAsToolStripMenuItem.Enabled = true;
                //btnSave.Enabled = true;
            }
            else
            {
                if (PackedFileName.Contains(".xml") || PackedFileName.Contains(".def") || PackedFileName.Contains(".visual") || PackedFileName.Contains(".chunk") || PackedFileName.Contains(".settings") || PackedFileName.Contains(".model"))
                {
                    saveAsToolStripMenuItem.Enabled = true;
                    btnSave.Enabled = true;
                    txtOut.LoadFile(file, RichTextBoxStreamType.PlainText);
                }
                else
                {
                    saveAsToolStripMenuItem.Enabled = false;
                    btnSave.Enabled = false;
                    throw new IOException("Invalid header");
                }
            }
            reader.Close();
            F.Close();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog { Filter = "WoT Packed XML|*.xml;*.def;*.visual;*.chunk;*.settings;*.primitives;*.model;*.animation;*.anca|All files|*.*" })
                if (DialogResult.OK == ofd.ShowDialog())
                {                    
                    openFile(ofd.FileName);
                }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var sfd = new SaveFileDialog { })
            {
                sfd.Filter = "Unpacked XML|*.xml";
                string SaveFileName = PackedFileName;
                if (!PackedFileName.Contains(".xml"))
                    SaveFileName = SaveFileName + ".xml";

                sfd.FileName = SaveFileName;
                if (DialogResult.OK == sfd.ShowDialog())
                {
                    xDoc.Save(sfd.FileName);
                    //txtOut.SaveFile(sfd.FileName, RichTextBoxStreamType.UnicodePlainText);
                }
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.Text = stitle + sver;
            this.DragEnter += new DragEventHandler(MainForm_DragEnter);
            this.DragDrop += new DragEventHandler(MainForm_DragDrop);
        }

        void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy; 
            else
                e.Effect = DragDropEffects.None; 

        }

        void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            string[] FileList = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            foreach (string File in FileList)
                openFile(File);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var abd = new AboutBox { })
            {
                if (DialogResult.Cancel == abd.ShowDialog())
                {
                }
            }
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveAsToolStripMenuItem.Enabled = false;
            btnSave.Enabled = false;
            txtOut.Clear();
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //
        }
    }
}
