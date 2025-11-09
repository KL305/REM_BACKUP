using REM_System.Data;
using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace REM_System.Forms
{
    public sealed class SellerDashboardForm : Form
    {
        private readonly DataGridView _dgvProperties = new DataGridView();
        private readonly Button _btnAddProperty = new Button();
        private readonly Button _btnEditProperty = new Button();
        private readonly Button _btnDeleteProperty = new Button();
        private readonly Button _btnRefresh = new Button();
        private readonly Button _btnLogout = new Button();
        private readonly Label _lblWelcome = new Label();
        private readonly Label _lblStatus = new Label();
        private readonly int _sellerId;
        private readonly string _username;
        private PropertyRepository _propertyRepo;

        public SellerDashboardForm(int sellerId, string username)
        {
            _sellerId = sellerId;
            _username = username;
            _propertyRepo = new PropertyRepository();
            InitializeForm();
            LoadProperties();
        }

        private void InitializeForm()
        {
            Text = $"Seller Dashboard - {_username}";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1000, 600);
            FormBorderStyle = FormBorderStyle.Sizable;
            MinimumSize = new Size(800, 500);

            _lblWelcome.Text = $"Welcome, {_username}!";
            _lblWelcome.AutoSize = true;
            _lblWelcome.Font = new Font(Font, FontStyle.Bold);
            _lblWelcome.Location = new Point(20, 20);

            _btnAddProperty.Text = "Add Property";
            _btnAddProperty.Location = new Point(20, 50);
            _btnAddProperty.Width = 120;
            _btnAddProperty.Height = 30;
            _btnAddProperty.Click += OnAddPropertyClick;

            _btnEditProperty.Text = "Edit Property";
            _btnEditProperty.Location = new Point(150, 50);
            _btnEditProperty.Width = 120;
            _btnEditProperty.Height = 30;
            _btnEditProperty.Click += OnEditPropertyClick;

            _btnDeleteProperty.Text = "Delete Property";
            _btnDeleteProperty.Location = new Point(280, 50);
            _btnDeleteProperty.Width = 120;
            _btnDeleteProperty.Height = 30;
            _btnDeleteProperty.Click += OnDeletePropertyClick;

            _btnRefresh.Text = "Refresh";
            _btnRefresh.Location = new Point(410, 50);
            _btnRefresh.Width = 100;
            _btnRefresh.Height = 30;
            _btnRefresh.Click += (s, e) => LoadProperties();

            _btnLogout.Text = "Logout";
            _btnLogout.Location = new Point(870, 20);
            _btnLogout.Width = 100;
            _btnLogout.Height = 30;
            _btnLogout.Click += (s, e) => DialogResult = DialogResult.OK;

            _dgvProperties.Location = new Point(20, 90);
            _dgvProperties.Width = 950;
            _dgvProperties.Height = 460;
            _dgvProperties.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _dgvProperties.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _dgvProperties.MultiSelect = false;
            _dgvProperties.ReadOnly = true;
            _dgvProperties.AllowUserToAddRows = false;
            _dgvProperties.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            _lblStatus.AutoSize = true;
            _lblStatus.ForeColor = Color.Green;
            _lblStatus.Location = new Point(20, 560);
            _lblStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

            Controls.Add(_lblWelcome);
            Controls.Add(_btnAddProperty);
            Controls.Add(_btnEditProperty);
            Controls.Add(_btnDeleteProperty);
            Controls.Add(_btnRefresh);
            Controls.Add(_btnLogout);
            Controls.Add(_dgvProperties);
            Controls.Add(_lblStatus);
        }

        private void LoadProperties()
        {
            try
            {
                var properties = _propertyRepo.GetPropertiesBySellerId(_sellerId);
                
                if (properties != null && properties.Count > 0)
                {
                    var propertyData = properties.Select(p => new
                    {
                        p.PropertyId,
                        p.Title,
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

                    // Configure columns safely after data binding
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
                else
                {
                    _dgvProperties.DataSource = null;
                    _dgvProperties.Rows.Clear();
                    _lblStatus.Text = "No properties found. Click 'Add Property' to create your first listing.";
                    _lblStatus.ForeColor = Color.Blue;
                }
            }
            catch (System.Data.SqlClient.SqlException sqlEx)
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

        private Property GetSelectedProperty()
        {
            if (_dgvProperties.SelectedRows.Count == 0)
                return null;

            var selectedRow = _dgvProperties.SelectedRows[0];
            if (selectedRow.DataBoundItem == null)
                return null;

            var propertyId = Convert.ToInt32(selectedRow.Cells["PropertyId"].Value);
            var properties = _propertyRepo.GetPropertiesBySellerId(_sellerId);
            return properties.FirstOrDefault(p => p.PropertyId == propertyId);
        }

        private void OnAddPropertyClick(object sender, EventArgs e)
        {
            using (var form = new AddPropertyForm(_sellerId))
            {
                if (form.ShowDialog(this) == DialogResult.OK && form.PropertyAdded)
                {
                    LoadProperties();
                    _lblStatus.Text = "Property added successfully!";
                    _lblStatus.ForeColor = Color.Green;
                }
            }
        }

        private void OnEditPropertyClick(object sender, EventArgs e)
        {
            var property = GetSelectedProperty();
            if (property == null)
            {
                MessageBox.Show("Please select a property to edit.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var form = new AddPropertyForm(_sellerId, property))
            {
                if (form.ShowDialog(this) == DialogResult.OK && form.PropertyAdded)
                {
                    LoadProperties();
                    _lblStatus.Text = "Property updated successfully!";
                    _lblStatus.ForeColor = Color.Green;
                }
            }
        }

        private void OnDeletePropertyClick(object sender, EventArgs e)
        {
            var property = GetSelectedProperty();
            if (property == null)
            {
                MessageBox.Show("Please select a property to delete.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to delete '{property.Title}'?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    if (_propertyRepo.DeleteProperty(property.PropertyId, _sellerId))
                    {
                        LoadProperties();
                        _lblStatus.Text = "Property deleted successfully!";
                        _lblStatus.ForeColor = Color.Green;
                    }
                    else
                    {
                        _lblStatus.Text = "Failed to delete property.";
                        _lblStatus.ForeColor = Color.Firebrick;
                    }
                }
                catch (Exception ex)
                {
                    _lblStatus.Text = $"Error deleting property: {ex.Message}";
                    _lblStatus.ForeColor = Color.Firebrick;
                }
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
                }

                var propertyTypeCol = _dgvProperties.Columns.Contains("PropertyType") ? _dgvProperties.Columns["PropertyType"] : null;
                if (propertyTypeCol != null)
                {
                    propertyTypeCol.HeaderText = "Type";
                }

                var descriptionCol = _dgvProperties.Columns.Contains("Description") ? _dgvProperties.Columns["Description"] : null;
                if (descriptionCol != null)
                {
                    descriptionCol.HeaderText = "Description";
                }

                var addressCol = _dgvProperties.Columns.Contains("Address") ? _dgvProperties.Columns["Address"] : null;
                if (addressCol != null)
                {
                    addressCol.HeaderText = "Address";
                }

                var priceCol = _dgvProperties.Columns.Contains("Price") ? _dgvProperties.Columns["Price"] : null;
                if (priceCol != null)
                {
                    priceCol.HeaderText = "Price";
                }

                var bedroomsCol = _dgvProperties.Columns.Contains("Bedrooms") ? _dgvProperties.Columns["Bedrooms"] : null;
                if (bedroomsCol != null)
                {
                    bedroomsCol.HeaderText = "Bedrooms";
                }

                var bathroomsCol = _dgvProperties.Columns.Contains("Bathrooms") ? _dgvProperties.Columns["Bathrooms"] : null;
                if (bathroomsCol != null)
                {
                    bathroomsCol.HeaderText = "Bathrooms";
                }

                var areaCol = _dgvProperties.Columns.Contains("Area") ? _dgvProperties.Columns["Area"] : null;
                if (areaCol != null)
                {
                    areaCol.HeaderText = "Area (sq ft)";
                }

                var statusCol = _dgvProperties.Columns.Contains("Status") ? _dgvProperties.Columns["Status"] : null;
                if (statusCol != null)
                {
                    statusCol.HeaderText = "Status";
                }

                var createdDateCol = _dgvProperties.Columns.Contains("CreatedDate") ? _dgvProperties.Columns["CreatedDate"] : null;
                if (createdDateCol != null)
                {
                    createdDateCol.HeaderText = "Created Date";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ConfigureColumns: {ex.Message}");
            }
        }
    }
}

