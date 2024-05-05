using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using static System.Net.WebRequestMethods;

namespace lab4
{
    public partial class Form4 : Form
    {
        HttpClient httpClient;
        DataTable dt = new DataTable();
        int Id = 0;
        public Form4()
        {
            InitializeComponent();
           
            dt.Columns.Add("Id");
            dt.Columns.Add("Name");
            dt.Columns.Add("Price");//sửa lại giá
            dt.Columns.Add("ManageStock");
            dt.Columns.Add("StockQuantity");
            dt.Columns.Add("StockStatus");
            dt.Columns.Add("Description");
        }
        private async void Form4_Load(object sender, EventArgs e)//xong
        {
            string consumerKey = "ck_781c50d23dc2cfc8e7a2ab069e4de72f51e1133d";
            string consumerSecret = "cs_7f15d43f6575cb1e905b51d3f6d5b8a333f61f99";
            string storeUrl = "http://localhost/shopat";

            // Create a new HttpClient with the base URL of the WooCommerce API
            httpClient = new HttpClient { BaseAddress = new Uri($"{storeUrl}/wp-json/wc/v3/") };

            // Add authentication headers
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{consumerKey}:{consumerSecret}")));
            await loadData(httpClient);

        }
        async Task loadData(HttpClient httpClient)
        {
            int page = 1;
            while (true)
            {
                var requestR = new HttpRequestMessage(HttpMethod.Get, $"products?page={page}&per_page=20");

                var responseR = await httpClient.SendAsync(requestR);
                var productsData = JsonConvert.DeserializeObject<dynamic>(await responseR.Content.ReadAsStringAsync());
                if (responseR.IsSuccessStatusCode)
                {
                    if (productsData.Count != 0)
                    {
                        foreach (var productData in productsData)
                        {
                            DataRow newRow = dt.NewRow();
                            newRow["Id"] = productData.id;
                            newRow["Name"] = productData.name;
                            newRow["Price"] = productData.price;
                            newRow["ManageStock"] = productData.manage_stock;
                            newRow["StockQuantity"] = productData.stock_quantity;
                            newRow["StockStatus"] = productData.stock_status;
                            newRow["Description"] = productData.description;
                            dt.Rows.Add(newRow);

                        }
                        page++;
                    }
                    else
                        break;
                }
                else
                {
                    Console.WriteLine($"Failed to read product: {responseR.StatusCode} - {await responseR.Content.ReadAsStringAsync()}");
                    break;
                }
                dataGridView1.DataSource = dt;
                foreach (DataGridViewColumn column in dataGridView1.Columns)
                {
                    if (column.Name != "Name" && column.Name != "Price")
                    {
                        column.Visible = false;
                    }
                }

            }
        }
        void setId(int x)
        {
            Id = x;
        }
        private void lb_add_Click(object sender, EventArgs e)
        {

        }
        private async void btn_add_ClickAsync(object sender, EventArgs e)
        {
            await HandleAddButtonClickAsync();
        }

        private async Task HandleAddButtonClickAsync()
        {
            var product = new
            {
                name = tb_name.Text,
                regular_price = tb_price.Text,
                description = rtb_descript.Text,
                manage_stock = "true",
                stock_quantity = Convert.ToInt16(tb_num.Text),
                stock_status = "instock",
                
            };
            var jsonProduct = JsonConvert.SerializeObject(product);

            // Create a new HttpRequestMessage with the product data
            var request = new HttpRequestMessage(HttpMethod.Post, "products")
            {
                Content = new StringContent(jsonProduct, Encoding.UTF8, "application/json")
            };

            // Send the request to the WooCommerce API
            var response = await httpClient.SendAsync(request);

            // If the request was successful, display the product ID that was added
            if (response.IsSuccessStatusCode)   
            {
                
                MessageBox.Show("thành công","",MessageBoxButtons.OK, MessageBoxIcon.Information);
                var responseBody = await response.Content.ReadAsStringAsync();
                var addedProduct = JsonConvert.DeserializeObject<dynamic>(responseBody);
                DataRow newRow = dt.NewRow();
                newRow["Id"] = addedProduct.id;
                newRow["Name"] = product.name;
                newRow["Price"] = product.regular_price;
                newRow["ManageStock"] = product.manage_stock;
                newRow["StockQuantity"] = product.stock_quantity;
                newRow["StockStatus"] = product.stock_status;
                newRow["Description"] = product.description; 
                dt.Rows.InsertAt(newRow,0);
                dataGridView1.DataSource = dt;
                dataGridView1.Refresh();
            }
            else
            {
                MessageBox.Show("Thêm sản phẩm không thành công!", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btn_del_ClickAsync(object sender, EventArgs e)
        {
            var responseD = await HandleDelButtonClickAsync();
            if (responseD.IsSuccessStatusCode)
            {
                MessageBox.Show("Xóa sản phẩm thành công!", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Xóa sản phẩm không thành công!", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            tb_name.Clear();
            tb_num.Clear();
            tb_price.Clear();
            rtb_descript.Clear();
        }

        private async Task<HttpResponseMessage> HandleDelButtonClickAsync()
        {
            // Create a new HttpRequestMessage with the product ID in the URL
            var requestD = new HttpRequestMessage(HttpMethod.Delete, $"products/{Id}");

            // Send the request to the WooCommerce API
            var responseD = await httpClient.SendAsync(requestD);

            // Handle the response based on success or failure
            if (responseD.IsSuccessStatusCode)
            {
                DataRow[] dataRow = dt.Select($"Id = {Id}");
                foreach (DataRow rowToDelete in dataRow)
                {
                    dt.Rows.Remove(rowToDelete);
                }
                dataGridView1.DataSource = dt;
            }
            return responseD;
            
            
        }


        private void btn_search_Click(object sender, EventArgs e)
        {
            DataTable table = new DataTable();
            table.Columns.Add("Id");
            table.Columns.Add("Name");
            table.Columns.Add("Price");//sửa lại giá
            table.Columns.Add("ManageStock");
            table.Columns.Add("StockQuantity");
            table.Columns.Add("StockStatus");
            table.Columns.Add("Description");
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                DataGridViewCell cell = row.Cells["Name"];
               if(!row.IsNewRow)
                {
                    if (cell.Value != null)
                    {
                        string name = (String)cell.Value;
                        bool kt = name.Contains(tb_search.Text);
                        if (kt) // Kiểm tra xem chuỗi name có chứa từ nhập vào không
                        {

                            MessageBox.Show("giống", "", MessageBoxButtons.OK, MessageBoxIcon.Error);

                            // Tạo một DataTable mới chứa một hàng từ DataGridViewRow
                            DataRow newRow = table.NewRow();
                            foreach (DataGridViewCell dgvCell in row.Cells)
                            {
                                newRow[dgvCell.OwningColumn.DataPropertyName] = dgvCell.Value;
                            }
                            table.Rows.Add(newRow);

                        }
                        else
                        {
                            MessageBox.Show("Không giống", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Có một ô không có giá trị!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }    
            }
            dataGridView1.DataSource = table;
        }


        private void btn_cancel_Click(object sender, EventArgs e)
        {
            tb_search.Clear();
            dataGridView1.DataSource = dt;
            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                if (column.Name != "Name" && column.Name != "Price")
                {
                    column.Visible = false;
                }
            }
        }

        private async void btn_update_ClickAsync(object sender, EventArgs e)//xong
        {
            await HandleUpdateButtonClickAsync();
        }

        private async Task HandleUpdateButtonClickAsync()
        {
            int rowIndex = dataGridView1.SelectedCells[0].RowIndex;
            DataGridViewRow row = dataGridView1.Rows[rowIndex];
            int productId = Convert.ToInt32(row.Cells["Id"].Value);
            var product = new
            {
                name = tb_name.Text,
                regular_price = tb_price.Text,
                description = rtb_descript.Text,
                manage_stock = "true",
                stock_quantity = Convert.ToInt16(tb_num.Text),
                stock_status = "instock",

            };
            // Convert the product data to JSON
            var jsonProductU = JsonConvert.SerializeObject(product);

            // Create a new HttpRequestMessage with the product data and ID in the URL
            var requestU = new HttpRequestMessage(HttpMethod.Put, $"products/{productId}")
            {
                Content = new StringContent(jsonProductU, Encoding.UTF8, "application/json")
            };

            // Send the request to the WooCommerce API
            var responseU = await httpClient.SendAsync(requestU);

            // Handle the response based on success or failure
            if (responseU.IsSuccessStatusCode)
            {
                MessageBox.Show("Cập nhật sản phẩm thành công!", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                if(product.stock_quantity ==0)
                {
                    var responseD = await HandleDelButtonClickAsync();
                    
                }
                else
                {
                    row.Cells["Name"].Value = product.name;
                    row.Cells["Price"].Value = product.regular_price;
                    row.Cells["ManageStock"].Value = product.manage_stock;
                    row.Cells["StockQuantity"].Value = product.stock_quantity;
                    row.Cells["StockStatus"].Value = product.stock_status;
                    row.Cells["Description"].Value = product.description;
                }
                // dataGridView1.Refresh();
            }
            else
            {
                Console.WriteLine($"Failed to update product: {responseU.StatusCode} - {await responseU.Content.ReadAsStringAsync()}");
            }


        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow selectedRow = dataGridView1.Rows[e.RowIndex];
                setId(Convert.ToInt16(selectedRow.Cells["Id"].Value));
                tb_name.Text = selectedRow.Cells["Name"].Value.ToString();
                tb_price.Text= selectedRow.Cells["Price"].Value.ToString();
                rtb_descript.Text = selectedRow.Cells["Description"].Value.ToString();
               tb_num.Text = selectedRow.Cells["StockQuantity"].Value.ToString();
            }
        }
    }
}
