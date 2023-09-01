using OfficeOpenXml;
using System;
using System.Data;
using System.IO;
using System.Windows.Forms;

namespace EEPlusExcelApp
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
        }

        
        private string DosyaAcVePathBul()
        {
            labelDosyaDurumu.Text = "Açılıyor...";
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            openFileDialog.Filter = "Excel Files|*.xlsx;*.xls";
            openFileDialog.ShowDialog();
            return openFileDialog.FileName;
        }
        private DataTable FilterByAge(DataTable originalTable, int minAge, int maxAge)
        {
            DataTable filteredTable = originalTable.Clone(); // Yeni bir tablo oluştur (sadece sütunlarını kopyala)

            foreach (DataRow row in originalTable.Rows)
            {
                int age = Convert.ToInt32(row["Age"]); // "Age" sütunundaki yaş değerini al
                if (age >= minAge && age <= maxAge)
                {
                    filteredTable.ImportRow(row); // Yaş aralığına uyan satırı kopyala
                }
            }

            return filteredTable;
        }
        private void SaveFilteredDataToWorksheet(ExcelPackage package, DataTable filteredData, string sheetName)
        {
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(sheetName); // Yeni bir sayfa oluştur

            // Başlık satırını ekle
            for (int col = 0; col < filteredData.Columns.Count; col++)
            {
                worksheet.Cells[1, col + 1].Value = filteredData.Columns[col].ColumnName;
            }

            // Verileri ekle
            for (int row = 0; row < filteredData.Rows.Count; row++)
            {
                for (int col = 0; col < filteredData.Columns.Count; col++)
                {
                    worksheet.Cells[row + 2, col + 1].Value = filteredData.Rows[row][col];
                }
            }
        }
        private void ShowFilteredDataInDataGridView2(DataTable filteredData)
        {
            dataGridView2.DataSource = filteredData;
        }



        private void btnFileOpen_Click(object sender, EventArgs e)
        {
                string filePath = DosyaAcVePathBul();
                groupBox1.Text = filePath;
                try
                {

                    using (var package = new ExcelPackage(new FileInfo(filePath)))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                        DataTable dataTable = new DataTable();

                        foreach (var firstRowCell in worksheet.Cells[1, 1, 1, worksheet.Dimension.End.Column])
                        {
                            dataTable.Columns.Add(firstRowCell.Text);
                        }

                        for (var rowNumber = 2; rowNumber <= worksheet.Dimension.End.Row; rowNumber++)
                        {
                            var row = worksheet.Cells[rowNumber, 1, rowNumber, worksheet.Dimension.End.Column];
                            var newRow = dataTable.NewRow();
                            foreach (var cell in row)
                            {
                                newRow[cell.Start.Column - 1] = cell.Text;
                            }
                            dataTable.Rows.Add(newRow);
                        }
                        dataGridView1.DataSource = dataTable;
                        labelDosyaDurumu.Text = "Açıldı";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Hata oluştu:" + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                comboBoxSheetNames.Items.Clear();
                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    foreach (var sheet in package.Workbook.Worksheets)
                    {
                        comboBoxSheetNames.Items.Add(sheet);
                    }
                }
                comboBoxSheetNames.SelectedIndex = 0;
            }

        private void btnFilterAge_Click(object sender, EventArgs e)
        {
            int minAge = 25; // Minimum yaş
            int maxAge = 35; // Maksimum yaş

            DataTable originalData = (DataTable)dataGridView1.DataSource; // Verilerin olduğu kaynak

            DataTable filteredData = FilterByAge(originalData, minAge, maxAge);

            ShowFilteredDataInDataGridView2(filteredData);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            int minAge = 25; // Minimum yaş
            int maxAge = 35; // Maksimum yaş

            string saveSheetName = textBoxSaveSheetName.Text;

            if (string.IsNullOrEmpty(saveSheetName))
            {
                MessageBox.Show("Lütfen kaydetmek istediğiniz sayfa adını girin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // İşlemi durdur
            }

            DataTable originalData = (DataTable)dataGridView1.DataSource; // Verilerin olduğu kaynak

            DataTable filteredData = FilterByAge(originalData, minAge, maxAge);

            string outputPath = @"C:\Users\muham\Desktop\Employee Sample Data.xlsx";

            using (var package = new ExcelPackage(new FileInfo(outputPath)))
            {
                ExcelWorksheet existingWorksheet = package.Workbook.Worksheets[saveSheetName];
                if (existingWorksheet != null)
                {
                    DialogResult result = MessageBox.Show("Girilen sayfa adında bir sayfa zaten var. Verileri üzerine yazmak istiyor musunuz?", "Uyarı", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (result == DialogResult.Yes)
                    {
                        package.Workbook.Worksheets.Delete(existingWorksheet);
                        SaveFilteredDataToWorksheet(package, filteredData, saveSheetName);

                        package.Save();
                        MessageBox.Show("Filtrelenmiş veriler yeni sayfaya kaydedildi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        // Kullanıcı "Hayır" dediğinde hiçbir işlem yapma
                        MessageBox.Show("Veriler üzerine yazma işlemi iptal edildi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {

                    SaveFilteredDataToWorksheet(package, filteredData, saveSheetName);
                    package.Save();
                    MessageBox.Show("Filtrelenmiş veriler yeni sayfaya kaydedildi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                // Filtrelenmiş verileri yeni bir sayfaya kaydetme
                //SaveFilteredDataToWorksheet(package, filteredData,saveSheetName);

                //package.Save();
            }

            //MessageBox.Show("Filtrelenmiş veriler yeni sayfaya kaydedildi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void comboBoxSheetNames_SelectedIndexChanged(object sender, EventArgs e)
        {
            string filePath = groupBox1.Text;
            string selectedSheetName = comboBoxSheetNames.SelectedItem.ToString();



            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[selectedSheetName];
                if (worksheet != null)
                {
                    DataTable dataTable = new DataTable();

                    foreach (var firstRowCell in worksheet.Cells[1, 1, 1, worksheet.Dimension.End.Column])
                    {
                        dataTable.Columns.Add(firstRowCell.Text);
                    }

                    for (var rowNumber = 2; rowNumber <= worksheet.Dimension.End.Row; rowNumber++)
                    {
                        var row = worksheet.Cells[rowNumber, 1, rowNumber, worksheet.Dimension.End.Column];
                        var newRow = dataTable.NewRow();
                        foreach (var cell in row)
                        {
                            newRow[cell.Start.Column - 1] = cell.Text;
                        }
                        dataTable.Rows.Add(newRow);
                    }
                    dataGridView1.DataSource = dataTable;
                    labelDosyaDurumu.Text = "Açıldı";
                }
            }
        }
    }
    }

