using REM_System.Data;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace REM_System.Forms
{
    public sealed class BuyerDashboardForm : Form
    {
        private readonly DataGridView _dgvProperties = new DataGridView();
        private readonly Button _btnRefresh = new Button();
        private readonly Button _btnLogout = new Button();
        private readonly ComboBox _cmbStatusFilter = new ComboBox();
        private readonly Label _lblWelcome = new Label();
        private readonly Label _lblStatus = new Label();
        private readonly Label _lblFilter = new Label();
        private readonly string _username;
        private PropertyRepository _propertyRepo;

        public BuyerDashboardForm(string username)
        {
            _username = username;
            _propertyRepo = new PropertyRepository();
            InitializeForm();
            
            // Delay loading properties until form is shown
            this.Shown += (s, e) => LoadProperties();
        }

        private void InitializeForm()
        {
            Text = $"Buyer Dashboard - {_username}";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1200, 650);
            FormBorderStyle = FormBorderStyle.Sizable;
            MinimumSize = new Size(1000, 550);

            _lblWelcome.Text = $"Welcome, {_username}! Browse Available Properties";
            _lblWelcome.AutoSize = true;
            _lblWelcome.Font = new Font("Microsoft Sans Serif", 9f, FontStyle.Bold);
            _lblWelcome.Location = new Point(20, 20);

            _lblFilter.Text = "Filter by Status:";
            _lblFilter.AutoSize = true;
            _lblFilter.Location = new Point(20, 50);

            _cmbStatusFilter.Location = new Point(120, 47);
            _cmbStatusFilter.Width = 150;
            _cmbStatusFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbStatusFilter.Items.AddRange(new object[] { "All", "Available", "Pending", "Sold" });
            _cmbStatusFilter.SelectedIndex = 0; // Default to "All"
            _cmbStatusFilter.SelectedIndexChanged += (s, e) => LoadProperties();

            _btnRefresh.Text = "Refresh";
            _btnRefresh.Location = new Point(290, 45);
            _btnRefresh.Width = 100;
            _btnRefresh.Height = 30;
            _btnRefresh.Click += (s, e) => LoadProperties();

            _btnLogout.Text = "Logout";
            _btnLogout.Location = new Point(1070, 20);
            _btnLogout.Width = 100;
            _btnLogout.Height = 30;
            _btnLogout.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _btnLogout.Click += (s, e) => DialogResult = DialogResult.OK;

            _dgvProperties.Location = new Point(20, 90);
            _dgvProperties.Width = 1150;
            _dgvProperties.Height = 520;
            _dgvProperties.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _dgvProperties.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _dgvProperties.MultiSelect = false;
            _dgvProperties.ReadOnly = true;
            _dgvProperties.AllowUserToAddRows = false;
            _dgvProperties.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            _dgvProperties.RowHeadersVisible = false;
            _dgvProperties.DefaultCellStyle.SelectionBackColor = Color.LightBlue;
            _dgvProperties.DefaultCellStyle.SelectionForeColor = Color.Black;

            _lblStatus.AutoSize = true;
            _lblStatus.ForeColor = Color.Green;
            _lblStatus.Location = new Point(20, 620);
            _lblStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

            Controls.Add(_lblWelcome);
            Controls.Add(_lblFilter);
            Controls.Add(_cmbStatusFilter);
            Controls.Add(_btnRefresh);
            Controls.Add(_btnLogout);
            Controls.Add(_dgvProperties);
            Controls.Add(_lblStatus);
        }

        private void LoadProperties()
        {
            try
            {
                if (_propertyRepo == null)
                {
                    _propertyRepo = new PropertyRepository();
                }

                if (_dgvProperties == null || _lblStatus == null)
                {
                    return;
                }

                string statusFilter = null;
                if (_cmbStatusFilter != null && _cmbStatusFilter.SelectedItem != null)
                {
                    var selectedStatus = Convert.ToString(_cmbStatusFilter.SelectedItem);
                    if (!string.IsNullOrEmpty(selectedStatus) && selectedStatus != "All")
                    {
                        statusFilter = selectedStatus;
                    }
                }

                var allProperties = _propertyRepo.GetAllPropertiesWithSeller();
                
                // Filter by status if needed
                var properties = allProperties;
                if (!string.IsNullOrEmpty(statusFilter))
                {
                    properties = allProperties.Where(p => p.Status == statusFilter).ToList();
                }

                if (properties != null && properties.Count > 0)
                {
                    try
                    {
                        var propertyData = properties.Select(p => new
                        {
                            p.PropertyId,
                            p.Title,
                            p.SellerName,
                            p.PropertyType,
                            Description = p.Description ?? string.Empty,
                            Address = p.Address ?? string.Empty,
                            Price = p.Price.ToString("C"),
                            Bedrooms = p.Bedrooms?.ToString() ?? "N/A",
                            Bathrooms = p.Bathrooms?.ToString() ?? "N/A",
                            Area = p.Area?.ToString("N2") ?? "N/A",
                            p.Status,
                            CreatedDate = p.CreatedDate.ToString("yyyy-MM-dd")
                        }).ToList();

                        _dgvProperties.DataSource = null;
                        _dgvProperties.DataSource = propertyData;

                        // Configure columns after data binding
                        DataGridViewBindingCompleteEventHandler bindingCompleteHandler = null;
                        bindingCompleteHandler = (s, e) =>
                        {
                            _dgvProperties.DataBindingComplete -= bindingCompleteHandler;
                            try
                            {
                                ConfigureColumns();
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error configuring columns: {ex.Message}");
                            }
                        };
                        _dgvProperties.DataBindingComplete += bindingCompleteHandler;
                        
                        // Try to configure immediately as fallback
                        try
                        {
                            ConfigureColumns();
                        }
                        catch
                        {
                            // Will be configured on DataBindingComplete event
                        }

                        _lblStatus.Text = $"Total Properties: {properties.Count}";
                        _lblStatus.ForeColor = Color.Green;
                    }
                    catch (Exception ex)
                    {
                        _lblStatus.Text = $"Error displaying properties: {ex.Message}";
                        _lblStatus.ForeColor = Color.Firebrick;
                    }
                }
                else
                {
                    _dgvProperties.DataSource = null;
                    _dgvProperties.Rows.Clear();
                    string selectedStatusText = statusFilter ?? "All";
                    _lblStatus.Text = selectedStatusText == "All" 
                        ? "No properties found." 
                        : $"No {selectedStatusText.ToLower()} properties found.";
                    _lblStatus.ForeColor = Color.Blue;
                }
            }
            catch (SqlException sqlEx)
            {
                if (sqlEx.Number == 208)
                {
                    _dgvProperties.DataSource = null;
                    _dgvProperties.Rows.Clear();
                    _lblStatus.Text = "Properties table not found. Please run the database setup script first.";
                    _lblStatus.ForeColor = Color.Firebrick;
                }
                else
                {
                    _lblStatus.Text = $"Database error: {sqlEx.Message}";
                    _lblStatus.ForeColor = Color.Firebrick;
                }
            }
            catch (Exception ex)
            {
                _lblStatus.Text = $"Error loading properties: {ex.Message}";
                _lblStatus.ForeColor = Color.Firebrick;
            }
        }

        private void ConfigureColumns()
        {
            try
            {
                if (_dgvProperties == null || _dgvProperties.Columns == null || _dgvProperties.Columns.Count == 0)
                    return;

                if (_dgvProperties.InvokeRequired)
                {
                    _dgvProperties.Invoke(new Action(ConfigureColumns));
                    return;
                }

                // Use safer column access
                var propertyIdCol = _dgvProperties.Columns.Contains("PropertyId") ? _dgvProperties.Columns["PropertyId"] : null;
                if (propertyIdCol != null)
                    propertyIdCol.Visible = false;

                var titleCol = _dgvProperties.Columns.Contains("Title") ? _dgvProperties.Columns["Title"] : null;
                if (titleCol != null)
                {
                    titleCol.HeaderText = "Title";
                    titleCol.Width = 200;
                }

                var sellerNameCol = _dgvProperties.Columns.Contains("SellerName") ? _dgvProperties.Columns["SellerName"] : null;
                if (sellerNameCol != null)
                {
                    sellerNameCol.HeaderText = "Seller";
                    sellerNameCol.Width = 120;
                }

                var propertyTypeCol = _dgvProperties.Columns.Contains("PropertyType") ? _dgvProperties.Columns["PropertyType"] : null;
                if (propertyTypeCol != null)
                {
                    propertyTypeCol.HeaderText = "Type";
                    propertyTypeCol.Width = 100;
                }

                var descriptionCol = _dgvProperties.Columns.Contains("Description") ? _dgvProperties.Columns["Description"] : null;
                if (descriptionCol != null)
                {
                    descriptionCol.HeaderText = "Description";
                    descriptionCol.Width = 250;
                }

                var addressCol = _dgvProperties.Columns.Contains("Address") ? _dgvProperties.Columns["Address"] : null;
                if (addressCol != null)
                {
                    addressCol.HeaderText = "Address";
                    addressCol.Width = 200;
                }

                var priceCol = _dgvProperties.Columns.Contains("Price") ? _dgvProperties.Columns["Price"] : null;
                if (priceCol != null)
                {
                    priceCol.HeaderText = "Price";
                    priceCol.Width = 100;
                }

                var bedroomsCol = _dgvProperties.Columns.Contains("Bedrooms") ? _dgvProperties.Columns["Bedrooms"] : null;
                if (bedroomsCol != null)
                {
                    bedroomsCol.HeaderText = "Bedrooms";
                    bedroomsCol.Width = 70;
                }

                var bathroomsCol = _dgvProperties.Columns.Contains("Bathrooms") ? _dgvProperties.Columns["Bathrooms"] : null;
                if (bathroomsCol != null)
                {
                    bathroomsCol.HeaderText = "Bathrooms";
                    bathroomsCol.Width = 70;
                }

                var areaCol = _dgvProperties.Columns.Contains("Area") ? _dgvProperties.Columns["Area"] : null;
                if (areaCol != null)
                {
                    areaCol.HeaderText = "Area (sq ft)";
                    areaCol.Width = 100;
                }

                var statusCol = _dgvProperties.Columns.Contains("Status") ? _dgvProperties.Columns["Status"] : null;
                if (statusCol != null)
                {
                    statusCol.HeaderText = "Status";
                    statusCol.Width = 80;
                }

                var createdDateCol = _dgvProperties.Columns.Contains("CreatedDate") ? _dgvProperties.Columns["CreatedDate"] : null;
                if (createdDateCol != null)
                {
                    createdDateCol.HeaderText = "Listed Date";
                    createdDateCol.Width = 100;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ConfigureColumns: {ex.Message}");
            }
        }
    }
}

