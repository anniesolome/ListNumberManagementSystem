using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;

namespace ListNumberManagementSystem
{
    public partial class Form1 : Form
    {
        public static DataTable ListDataTable { get; set; }
        public Form1()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.ShowDialog();                
                string SourceURl = "";
                if (dialog.FileName != "")
                {
                    if (dialog.FileName.EndsWith(".csv"))
                    {
                        DataTable dtNew = new DataTable();
                        //Reading the data from CSV file
                        dtNew = GetDataTabletFromCSVFile(dialog.FileName);
                        if (dtNew.Rows != null && dtNew.Rows.ToString() != String.Empty)
                        {
                            InsertData(dtNew);
                            ListDataTable = GetDataFromDB();
                            dataGridView1.Columns.Clear();
                            //Binding the data to DataGrid
                            dataGridView1.DataSource = ListDataTable;
                            //Adding Button to DataGrid
                            DataGridViewButtonColumn btn = new DataGridViewButtonColumn();
                            dataGridView1.Columns.Add(btn);
                            btn.HeaderText = "Action";
                            btn.Text = "Update ItemID";
                            btn.Name = "btn";
                            btn.UseColumnTextForButtonValue = true;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Selected File is Invalid, Please Select valid csv file.", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception " + ex);
            }
        }
        public static DataTable GetDataTabletFromCSVFile(string csv_file_path)
        {
            DataTable dt = new DataTable();
            try
            {                
                string CSVFilePathName = csv_file_path; 
                string[] Lines = File.ReadAllLines(CSVFilePathName);
                string[] Fields;
                Fields = Lines[0].Split(new char[] { ';' });
                int Cols = Fields.GetLength(0); 
                //First row in csv file represents the columns 
                //Adding columns to data table
                for (int i = 0; i < Cols; i++)
                    dt.Columns.Add(Fields[i].ToLower().Trim(), typeof(string));

                //After first row in csv file the remaining rows represent the data
                DataRow Row;
                for (int i = 1; i < Lines.GetLength(0); i++)
                {
                    Fields = Lines[i].Split(new char[] { ';' });
                    Row = dt.NewRow();

                    //Adding the values from Fields array to Row object
                    for (int f = 0; f < Cols; f++)
                        Row[f] = Fields[f];

                    //Adding the DatatRow object to Data Table
                    dt.Rows.Add(Row);
                }
                return  dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error is " + ex.ToString());
                throw;
            }
        }

        public void InsertData(DataTable dt)
        {
            //Saving the data in database
            string connectionName = "data source=DESKTOP-59RG64O\\SQLEXPRESS;Initial Catalog = Employee; Integrated Security = True;";
            SqlConnection con = new SqlConnection(connectionName);
            con.Open();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                SqlCommand cmd = new SqlCommand("Insert_EmployeeList", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Username", Convert.ToString(dt.Rows[i]["username"]));
                cmd.Parameters.AddWithValue("@Identifier", Convert.ToString(dt.Rows[i]["identifier"]));
                cmd.Parameters.AddWithValue("@Firstname", Convert.ToString(dt.Rows[i]["first name"]));
                cmd.Parameters.AddWithValue("@Lastname", Convert.ToString(dt.Rows[i]["last name"]));                
                cmd.ExecuteNonQuery();
            }
            con.Close();
        }
        public void UpdateData(DataTable dt)
        {
            string connectionName = "data source=DESKTOP-59RG64O\\SQLEXPRESS;Initial Catalog = Employee; Integrated Security = True;";
            SqlConnection con = new SqlConnection(connectionName);
            con.Open();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                SqlCommand cmd = new SqlCommand("Update_EmployeeList", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Username", Convert.ToString(dt.Rows[i]["ListNumber"]));
                cmd.Parameters.AddWithValue("@ItemID", Convert.ToInt32(textBox1.Text.Trim())); ;
                cmd.ExecuteNonQuery();
            }
            con.Close();
        }
        public DataTable GetDataFromDB()
        {
            string connectionName = "data source=DESKTOP-59RG64O\\SQLEXPRESS;Initial Catalog = Employee; Integrated Security = True;";
            SqlConnection con = new SqlConnection(connectionName);
            SqlDataAdapter da = new SqlDataAdapter("Get_EmployeeList", con);
            da.SelectCommand.CommandType = CommandType.StoredProcedure;
            DataSet ds = new DataSet();
            da.Fill(ds);
            return ds.Tables[0];
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (!(textBox1.TextLength == 4))
                {
                    MessageBox.Show("Please Enter 4 Digit Item ID ", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBox1.Text = "";
                }
                if (ListDataTable != null)
                {
                    UpdateData(ListDataTable);
                    MessageBox.Show("Item ID successfully Updated..!", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    ListDataTable = GetDataFromDB();
                    dataGridView1.Columns.Clear();
                    dataGridView1.DataSource = ListDataTable;

                    DataGridViewButtonColumn btn = new DataGridViewButtonColumn();
                    dataGridView1.Columns.Add(btn);
                    btn.HeaderText = "Action";
                    btn.Text = "Update ItemID";
                    btn.Name = "btn";
                    btn.UseColumnTextForButtonValue = true;
                }
                else
                {
                    MessageBox.Show("Please Import the Data from CSV File!", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.ToString());
            }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            //Validating the Item ID text box where it allows only integer values
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DataTable dt = GetItemIDFromDB();
            if (dt.Rows.Count > 0)
            {
                dataGridView1.Columns.Clear();
                dataGridView1.DataSource = null;
                dataGridView1.DataSource = dt;
            }
            else
            {
                MessageBox.Show("No Data Available!", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public DataTable GetItemIDFromDB()
        {
            DataTable dt = null;
            try
            {
                string connectionName = "data source=DESKTOP-59RG64O\\SQLEXPRESS;Initial Catalog = Employee; Integrated Security = True;";
                SqlConnection con = new SqlConnection(connectionName);
                SqlDataAdapter da = new SqlDataAdapter("Get_ItemID", con);
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                DataSet ds = new DataSet();
                da.Fill(ds);
                dt = ds.Tables[0];
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            return dt;
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (!(textBox1.TextLength == 4))
                {
                    MessageBox.Show("Please Enter 4 Digit Item ID ", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBox1.Text = "";
                    return;
                }
                if (dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value != null)
                {
                    dataGridView1.CurrentRow.Selected = true;
                    string listNumber = dataGridView1.Rows[e.RowIndex].Cells["ListNumber"].FormattedValue.ToString();
                    string connectionName = "data source=DESKTOP-59RG64O\\SQLEXPRESS;Initial Catalog = Employee; Integrated Security = True;";
                    SqlConnection con = new SqlConnection(connectionName);
                    con.Open();
                    SqlCommand cmd = new SqlCommand("Update_EmployeeList", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Username", listNumber);
                    cmd.Parameters.AddWithValue("@ItemID", Convert.ToInt32(textBox1.Text.Trim())); ;
                    cmd.ExecuteNonQuery();
                    con.Close();
                    MessageBox.Show("Item ID successfully Updated..!", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    ListDataTable = GetDataFromDB();
                    dataGridView1.DataSource = ListDataTable;
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.ToString());
            }
        }
    }
}
