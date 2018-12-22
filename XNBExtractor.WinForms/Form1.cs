using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Graphics;

namespace XNBExtractor.WinForms
{
    public partial class Form1 : Form
    {
        private readonly Extractor extractor = new Extractor();
        private bool isConverting = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.DataSource = new[]
            {
                new { Id = Extractor.AssetType.Texture2D, Text = "2D Texture" },
                new { Id = Extractor.AssetType.TextureCube, Text = "Cube Texture" },
                //new { Id = Extractor.AssetType.MusicOrSound, Text = "Music/Sounds" },
            };
            comboBox1.DisplayMember = "Text";
            comboBox1.ValueMember = "Id";

            comboBox2.DataSource = new[]
            {
                new { Id = GraphicsProfile.HiDef, Text = "HiDef" },
                new { Id = GraphicsProfile.Reach, Text = "Reach" },
            };
            comboBox2.DisplayMember = "Text";
            comboBox2.ValueMember = "Id";
            comboBox2.SelectedValueChanged += (comboBox, _) =>
                Extractor.GraphicsProfile = (GraphicsProfile)((ComboBox)comboBox).SelectedValue;

            comboBox3.DataSource = new[]
            {
                new { Id = Extractor.ImageExtension.Png, Text = ".png" },
                new { Id = Extractor.ImageExtension.Jpeg, Text = ".jpeg" },
            };
            comboBox3.DisplayMember = "Text";
            comboBox3.ValueMember = "Id";
            comboBox3.SelectedValueChanged += (comboBox, _) => 
                extractor.SaveImagesAs = (Extractor.ImageExtension)((ComboBox)comboBox).SelectedValue;

            label4.Left = (ClientSize.Width - label4.Width) / 2;
            label4.Top = (ClientSize.Height - label4.Height) / 2;

            MinimumSize = new Size(Size.Width, Size.Height / 2);

            Extractor.GraphicsProfile = GraphicsProfile.HiDef;
            extractor.Initialize();
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            isConverting = true;
            comboBox1.Enabled = comboBox2.Enabled = comboBox3.Enabled = false;

            var files = (string[])e.Data.GetData(DataFormats.FileDrop);

            extractor.Initialize();
            extractor.ProcessFiles((Extractor.AssetType)comboBox1.SelectedValue, files);

            comboBox1.Enabled = comboBox2.Enabled = comboBox3.Enabled = true;
            isConverting = false;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            extractor.Dispose();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (isConverting)
            {
                e.Cancel = true;
            }
        }
    }
}
