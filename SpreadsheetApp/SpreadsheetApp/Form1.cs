using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpreadsheetApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            mySpreedsheet = new SharableSpreadSheet(1, 1);
            updateDataGrid();
            buttonLoad.Enabled = false;
            buttonSave.Enabled = false;
            activeLoad = false;
            buttonAddRow.Enabled = false;
            buttonAddColumn.Enabled = false;
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (!activeLoad)
            {
                if (dataGridView1[e.ColumnIndex, e.RowIndex].Value == null)
                    mySpreedsheet.setCell(e.RowIndex, e.ColumnIndex, null);
                else
                    mySpreedsheet.setCell(e.RowIndex, e.ColumnIndex, dataGridView1[e.ColumnIndex, e.RowIndex].Value.ToString());
            }
        }


        private void buttonLoad_Click(object sender, EventArgs e)
        {
            String file = textBox1.Text;

            if (File.Exists(file) && ((File.GetAttributes(file) & FileAttributes.Directory) != FileAttributes.Directory))
            {
                try
                {
                    mySpreedsheet.load(file);
                }
                catch (IOException)
                {
                    MessageBox.Show("The file is not in the correct format", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                activeLoad = true;
                updateDataGrid();
                activeLoad = false;
            }
            else
                MessageBox.Show("Please insert a valid file path", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);



        }

        private void updateDataGrid()
        {
            Tuple<int, int> sizes = mySpreedsheet.getSize();

            this.dataGridView1.DataSource = null;
            this.dataGridView1.Rows.Clear();
            dataGridView1.ColumnCount = sizes.Item2;

            for (int i=0; i<sizes.Item1; i++)
            {
                DataGridViewRow row = new DataGridViewRow();
                row.CreateCells(this.dataGridView1);

                for (int j=0; j < sizes.Item2; j++)
                    row.Cells[j].Value = mySpreedsheet.matrix[i][j].ToString();
                this.dataGridView1.Rows.Add(row);
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            String file = textBox1.Text;

            if (File.Exists(file) && ((File.GetAttributes(file) & FileAttributes.Directory) != FileAttributes.Directory))
            {
                try
                {
                    mySpreedsheet.save(file);
                }
                catch (IOException)
                {
                    MessageBox.Show("Save could not be done", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            else
            {
                MessageBoxButtons Mbuttons = MessageBoxButtons.YesNo;
                DialogResult Mresult =  MessageBox.Show("Are you sure you want to create a new file?", "Save New File", Mbuttons);
                if (Mresult == DialogResult.Yes)
                {
                    try
                    {
                        mySpreedsheet.save(file);
                    }
                    catch (IOException)
                    {
                        MessageBox.Show("Save could not be done", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                else
                    return;
            }
            MessageBox.Show("Saved Succesfully", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);

        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            string file = "";
            int size = -1;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            DialogResult result = openFileDialog1.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                file = openFileDialog1.FileName;
            }
            textBox1.Text = file;
            //Console.WriteLine(size); // <-- Shows file size in debugging mode.
            //Console.WriteLine(result); // <-- For debugging use.
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.TextLength == 0)
            {
                buttonLoad.Enabled = false;
                buttonSave.Enabled = false;
            }
            else
            {
                buttonLoad.Enabled = true;
                buttonSave.Enabled = true;
            }
        }

        private void buttonAddRow_Click(object sender, EventArgs e)
        {
            int index = 0;
            Tuple<int, int> sizes = mySpreedsheet.getSize();

            try
            {
                index = Int32.Parse(textBox2.Text);
                if (index > sizes.Item1)
                    index = sizes.Item1;
            }
            catch (Exception)
            {
                MessageBox.Show("not a valid number", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            mySpreedsheet.addRow(index);
            updateDataGrid();
        }

        private void buttonAddColumn_Click(object sender, EventArgs e)
        {
            int index = 0;
            Tuple<int, int> sizes = mySpreedsheet.getSize();

            try
            {
                index = Int32.Parse(textBox2.Text);
                if (index > sizes.Item2)
                    index = sizes.Item2;
            }
            catch (Exception)
            {
                MessageBox.Show("not a valid number", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            mySpreedsheet.addCol(index);
            updateDataGrid();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (textBox2.TextLength == 0)
            {
                buttonAddRow.Enabled = false;
                buttonAddColumn.Enabled = false;
            }
            else
            {
                buttonAddRow.Enabled = true;
                buttonAddColumn.Enabled = true;
            }
        }

        private void buttonSearch_Click(object sender, EventArgs e)
        {
            if (textBox3.Text.Length == 0)
            {
                try
                {
                    mySpreedsheet.setAll(null, textBox4.Text, false);
                }
                catch (Exception)
                {
                    try
                    {
                        mySpreedsheet.setAll(textBox3.Text, textBox4.Text, false);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("could not find the word ro replace", "No Results", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    updateDataGrid();
                    return;
                }
                try
                {
                    mySpreedsheet.setAll(textBox3.Text, textBox4.Text, false);
                }
                catch (Exception)
                {
                }
            }
            else
            {
                try
                {
                    mySpreedsheet.setAll(textBox3.Text, textBox4.Text, false);
                }
                catch (Exception)
                {
                    MessageBox.Show("could not find the word to replace", "No Results", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            updateDataGrid();
        }

        private void buttonSearch_Click_1(object sender, EventArgs e)
        {
            dataGridView1.ClearSelection();
            List<Tuple<int, int>> cellList = new List<Tuple<int, int>>();

            if (textSearch.Text.Length == 0)
            {
                try
                {
                    cellList.AddRange(mySpreedsheet.findAll(null, false));
                }
                catch (Exception)
                {
                    try
                    {
                        cellList.AddRange(mySpreedsheet.findAll(textSearch.Text, false));
                        foreach (Tuple<int, int> cell in cellList)
                        {
                            dataGridView1.Rows[cell.Item1].Cells[cell.Item2].Selected = true;
                        }
                        return;
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("could not find the word", "No Results", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                try
                {
                    cellList.AddRange(mySpreedsheet.findAll(textSearch.Text, false));
                }
                catch (Exception)
                {
                }
            }
            else
            {
                try
                {
                    cellList.AddRange(mySpreedsheet.findAll(textSearch.Text, false));
                }
                catch (Exception)
                {
                    MessageBox.Show("could not find the word", "No Results", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            foreach (Tuple<int, int> cell in cellList)
            {
                dataGridView1.Rows[cell.Item1].Cells[cell.Item2].Selected = true;
            }
        }
    }
}
