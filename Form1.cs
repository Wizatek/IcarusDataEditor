using System;
using System.Windows.Forms;

namespace IcarusDataEditor
{
    public partial class Form1 : Form
    {
        BinFile binFile;
        string openedFile;

        public Form1()
        {
            InitializeComponent();
            dgData.DoubleBuffered(true);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Icarus Data (*.bin)|*.bin|All Files|*.*";

            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            binFile = new BinFile();
            if(!binFile.ReadFile(ofd.FileName))
            {
                lblStatus.Text = $"Failed to load file [{ofd.FileName}]";
                return;
            }

            openedFile = ofd.FileName;

            fillGrid();

            lblStatus.Text = $"Loaded [{openedFile}]";
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFile(openedFile);
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Icarus Data (*.bin)|*.bin|All Files|*.*";

            if (sfd.ShowDialog() != DialogResult.OK)
                return;

            openedFile = sfd.FileName;
            saveFile(openedFile);
        }

        private void ecitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void fillGrid()
        {
            dgData.Columns.Clear();
            dgData.Rows.Clear();

            int rowCount = binFile.GetRowCount();
            int colCount = binFile.GetColCount();

            for (int i = 0; i < colCount; i++)
                dgData.Columns.Add("Col" + i, binFile.GetName(i));


            for (int row = 0; row < rowCount; row++)
            {
                dgData.Rows.Add();
                for (int col = 0; col < colCount; col++)
                {
                    switch (binFile.GetFieldType(col))
                    {
                        case BinFile.FIELD_TYPE_FLOAT:
                            {
                                float data = binFile.GetFloat(row, col);
                                dgData[col, row].Value = data;
                            }
                            break;

                        case BinFile.FIELD_TYPE_STRING:
                            {
                                string data = binFile.GetString(row, col);
                                dgData[col, row].Value = data;
                            }
                            break;
                    }
                }
            }

            for (int i = 0; i < colCount; i++)
                dgData.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            

        }

        private void tsbtnChange_Click(object sender, EventArgs e)
        {
            if(binFile == null)
            {
                lblStatus.Text = "No BIN file opened";
                return;
            }

            string encoding = tstbEncoding.Text;
            if (!binFile.SetEncoding(encoding))
            {
                lblStatus.Text = $"Encoding {encoding} can not be used, typo?";
                return;
            }

            fillGrid();
        }

        private void tstbEncoding_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                tsbtnChange_Click(sender, null);
        }

        private void saveData()
        {
            int rowCount = dgData.RowCount - 1;
            int colCount = dgData.ColumnCount;

            object[] data = new object[rowCount * colCount];

            for (int row = 0; row < rowCount; row++)
            {
                for (int col = 0; col < colCount; col++)
                    data[(row * colCount) + col] = dgData[col, row].Value;
            }

            binFile.SetData(rowCount, colCount, data);
        }

        private void saveFile(string fileName)
        {
            if (binFile == null)
            {
                lblStatus.Text = "Error: No bin file opened";
                return;
            }

            saveData();
            if(!binFile.SaveFile(fileName))
            {
                lblStatus.Text = $"Error: Cannot save file [{fileName}]";
                return;
            }

            lblStatus.Text = $"File Saved: {fileName}";
        }
    }
}
