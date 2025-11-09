using REM_System.Data;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace REM_System.Forms
{
    public sealed class AdminDashboardForm : Form
    {
        private readonly TabControl _tabControl = new TabControl();
        private readonly DataGridView _dgvUsers = new DataGridView();
        private readonly DataGridView _dgvProperties = new DataGridView();
        private readonly Button _btnRefreshUsers = new Button();
        private readonly Button _btnAddUser = new Button();
        private readonly Button _btnEditUser = new Button();
        private readonly Button _btnDeleteUser = new Button();
        private readonly Button _btnRefreshProperties = new Button();
        private readonly Button _btnAddProperty = new Button();
        private readonly Button _btnEditProperty = new Button();
        private readonly Button _btnDeleteProperty = new Button();
        private readonly Button _btnLogout = new Button();
        private readonly Label _lblWelcome = new Label();
        private readonly Label _lblUsersStatus = new Label();
        private readonly Label _lblPropertiesStatus = new Label();
        private readonly string _username;
        private UserRepository _userRepo;
        private PropertyRepository _propertyRepo;

        public AdminDashboardForm(string username)
        {
            _username = username;
            InitializeForm();
            
            // Initialize repositories in Load event
            this.Load += (s, e) =>
            {
                _userRepo = new UserRepository();
                _propertyRepo = new PropertyRepository();
            };
            
            // Delay loading data until form is fully shown and rendered
            this.Shown += (s, e) =>
            {
                // Use BeginInvoke to ensure UI is fully rendered
                this.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        LoadUsers();
                        LoadProperties();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error loading data in AdminDashboardForm: {ex.Message}");
                        MessageBox.Show($"Error loading dashboard data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }));
            };
        }

        private void InitializeForm()
        {
            Text = $"Admin Dashboard - {_username}";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1200, 700);
            FormBorderStyle = FormBorderStyle.Sizable;
            MinimumSize = new Size(1000, 600);

            _lblWelcome.Text = $"Welcome, {_username}! System Administration";
            _lblWelcome.AutoSize = true;
            _lblWelcome.Font = new Font("Microsoft Sans Serif", 10f, FontStyle.Bold);
            _lblWelcome.Location = new Point(20, 20);

            _btnLogout.Text = "Logout";
            _btnLogout.Location = new Point(1070, 20);
            _btnLogout.Width = 100;
            _btnLogout.Height = 30;
            _btnLogout.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _btnLogout.Click += (s, e) => DialogResult = DialogResult.OK;

            // Tab Control
            _tabControl.Location = new Point(20, 60);
            _tabControl.Width = 1150;
            _tabControl.Height = 600;
            _tabControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            // Users Tab
            var tabUsers = new TabPage("Users");
            tabUsers.UseVisualStyleBackColor = true;

            _btnAddUser.Text = "Add User";
            _btnAddUser.Location = new Point(20, 10);
            _btnAddUser.Width = 100;
            _btnAddUser.Height = 30;
            _btnAddUser.Click += OnAddUserClick;

            _btnEditUser.Text = "Edit User";
            _btnEditUser.Location = new Point(130, 10);
            _btnEditUser.Width = 100;
            _btnEditUser.Height = 30;
            _btnEditUser.Click += OnEditUserClick;

            _btnDeleteUser.Text = "Delete User";
            _btnDeleteUser.Location = new Point(240, 10);
            _btnDeleteUser.Width = 100;
            _btnDeleteUser.Height = 30;
            _btnDeleteUser.Click += OnDeleteUserClick;

            _btnRefreshUsers.Text = "Refresh";
            _btnRefreshUsers.Location = new Point(350, 10);
            _btnRefreshUsers.Width = 100;
            _btnRefreshUsers.Height = 30;
            _btnRefreshUsers.Click += (s, e) => LoadUsers();

            _dgvUsers.Location = new Point(20, 50);
            _dgvUsers.Width = 1100;
            _dgvUsers.Height = 480;
            _dgvUsers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _dgvUsers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _dgvUsers.MultiSelect = false;
            _dgvUsers.ReadOnly = true;
            _dgvUsers.AllowUserToAddRows = false;
            _dgvUsers.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            _dgvUsers.RowHeadersVisible = false;
            _dgvUsers.DefaultCellStyle.SelectionBackColor = Color.LightBlue;
            _dgvUsers.DefaultCellStyle.SelectionForeColor = Color.Black;

            _lblUsersStatus.AutoSize = true;
            _lblUsersStatus.ForeColor = Color.Green;
            _lblUsersStatus.Location = new Point(20, 540);
            _lblUsersStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

            tabUsers.Controls.Add(_btnAddUser);
            tabUsers.Controls.Add(_btnEditUser);
            tabUsers.Controls.Add(_btnDeleteUser);
            tabUsers.Controls.Add(_btnRefreshUsers);
            tabUsers.Controls.Add(_dgvUsers);
            tabUsers.Controls.Add(_lblUsersStatus);

            // Properties Tab
            var tabProperties = new TabPage("Properties");
            tabProperties.UseVisualStyleBackColor = true;

            _btnAddProperty.Text = "Add Property";
            _btnAddProperty.Location = new Point(20, 10);
            _btnAddProperty.Width = 100;
            _btnAddProperty.Height = 30;
            _btnAddProperty.Click += OnAddPropertyClick;

            _btnEditProperty.Text = "Edit Property";
            _btnEditProperty.Location = new Point(130, 10);
            _btnEditProperty.Width = 100;
            _btnEditProperty.Height = 30;
            _btnEditProperty.Click += OnEditPropertyClick;

            _btnDeleteProperty.Text = "Delete Property";
            _btnDeleteProperty.Location = new Point(240, 10);
            _btnDeleteProperty.Width = 100;
            _btnDeleteProperty.Height = 30;
            _btnDeleteProperty.Click += OnDeletePropertyClick;

            _btnRefreshProperties.Text = "Refresh";
            _btnRefreshProperties.Location = new Point(350, 10);
            _btnRefreshProperties.Width = 100;
            _btnRefreshProperties.Height = 30;
            _btnRefreshProperties.Click += (s, e) => LoadProperties();

            _dgvProperties.Location = new Point(20, 50);
            _dgvProperties.Width = 1100;
            _dgvProperties.Height = 480;
            _dgvProperties.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _dgvProperties.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _dgvProperties.MultiSelect = false;
            _dgvProperties.ReadOnly = true;
            _dgvProperties.AllowUserToAddRows = false;
            _dgvProperties.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            _dgvProperties.RowHeadersVisible = false;
            _dgvProperties.DefaultCellStyle.SelectionBackColor = Color.LightBlue;
            _dgvProperties.DefaultCellStyle.SelectionForeColor = Color.Black;

            _lblPropertiesStatus.AutoSize = true;
            _lblPropertiesStatus.ForeColor = Color.Green;
            _lblPropertiesStatus.Location = new Point(20, 540);
            _lblPropertiesStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

            tabProperties.Controls.Add(_btnAddProperty);
            tabProperties.Controls.Add(_btnEditProperty);
            tabProperties.Controls.Add(_btnDeleteProperty);
            tabProperties.Controls.Add(_btnRefreshProperties);
            tabProperties.Controls.Add(_dgvProperties);
            tabProperties.Controls.Add(_lblPropertiesStatus);

            _tabControl.TabPages.Add(tabUsers);
            _tabControl.TabPages.Add(tabProperties);

            Controls.Add(_lblWelcome);
            Controls.Add(_btnLogout);
            Controls.Add(_tabControl);
        }

        private void LoadUsers()
        {
            try
            {
                if (_userRepo == null)
                {
                    _userRepo = new UserRepository();
                }

                if (_dgvUsers == null || _lblUsersStatus == null)
                {
                    System.Diagnostics.Debug.WriteLine("LoadUsers: Controls not initialized in AdminDashboardForm");
                    return;
                }

                // Ensure we're on the UI thread
                if (_dgvUsers.InvokeRequired)
                {
                    _dgvUsers.Invoke(new Action(LoadUsers));
                    return;
                }

                var allUsers = _userRepo.GetAllUsers();

                if (allUsers == null)
                {
                    allUsers = new List<UserViewModel>();
                }

                // Filter to show only Buyers and Sellers, exclude Admins
                var users = allUsers.Where(u => u.UserRole == "Buyer" || u.UserRole == "Seller").ToList();

                if (users.Count > 0)
                {
                    try
                    {
                        var userData = users.Select(u => new
                        {
                            u.UserId,
                            u.Username,
                            u.Email,
                            u.UserRole
                        }).ToList();

                        _dgvUsers.DataSource = null;
                        _dgvUsers.DataSource = userData;

                        // Configure columns after data binding
                        DataGridViewBindingCompleteEventHandler bindingCompleteHandler = null;
                        bindingCompleteHandler = (s, e) =>
                        {
                            _dgvUsers.DataBindingComplete -= bindingCompleteHandler;
                            try
                            {
                                ConfigureUserColumns();
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error configuring user columns: {ex.Message}");
                            }
                        };
                        _dgvUsers.DataBindingComplete += bindingCompleteHandler;

                        // Try to configure immediately as fallback
                        try
                        {
                            ConfigureUserColumns();
                        }
                        catch
                        {
                            // Will be configured on DataBindingComplete event
                        }

                        _lblUsersStatus.Text = $"Total Users: {users.Count}";
                        _lblUsersStatus.ForeColor = Color.Green;
                    }
                    catch (Exception ex)
                    {
                        _lblUsersStatus.Text = $"Error displaying users: {ex.Message}";
                        _lblUsersStatus.ForeColor = Color.Firebrick;
                        System.Diagnostics.Debug.WriteLine($"Error in LoadUsers display: {ex.Message}");
                    }
                }
                else
                {
                    _dgvUsers.DataSource = null;
                    _dgvUsers.Rows.Clear();
                    _lblUsersStatus.Text = "No users found.";
                    _lblUsersStatus.ForeColor = Color.Blue;
                }
            }
            catch (SqlException sqlEx)
            {
                if (sqlEx.Number == 208) // Invalid object name
                {
                    _dgvUsers.DataSource = null;
                    _dgvUsers.Rows.Clear();
                    _lblUsersStatus.Text = "Users table not found. Please run the database setup script first.";
                    _lblUsersStatus.ForeColor = Color.Firebrick;
                }
                else
                {
                    _lblUsersStatus.Text = $"Database error: {sqlEx.Message}";
                    _lblUsersStatus.ForeColor = Color.Firebrick;
                }
            }
            catch (Exception ex)
            {
                _lblUsersStatus.Text = $"Error loading users: {ex.Message}";
                _lblUsersStatus.ForeColor = Color.Firebrick;
                System.Diagnostics.Debug.WriteLine($"Error in LoadUsers: {ex.Message}");
            }
        }

        private void LoadProperties()
        {
            try
            {
                if (_propertyRepo == null)
                {
                    _propertyRepo = new PropertyRepository();
                }

                if (_dgvProperties == null || _lblPropertiesStatus == null)
                {
                    System.Diagnostics.Debug.WriteLine("LoadProperties: Controls not initialized in AdminDashboardForm");
                    return;
                }

                // Ensure we're on the UI thread
                if (_dgvProperties.InvokeRequired)
                {
                    _dgvProperties.Invoke(new Action(LoadProperties));
                    return;
                }

                var properties = _propertyRepo.GetAllPropertiesWithSeller();

                if (properties == null)
                {
                    properties = new List<PropertyViewModel>();
                }

                if (properties.Count > 0)
                {
                    try
                    {
                        var propertyData = properties.Select(p => new
                        {
                            p.PropertyId,
                            p.Title,
                            p.SellerName,
                            Description = p.Description ?? string.Empty,
                            p.PropertyType,
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
                                ConfigurePropertyColumns();
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error configuring property columns: {ex.Message}");
                            }
                        };
                        _dgvProperties.DataBindingComplete += bindingCompleteHandler;

                        // Try to configure immediately as fallback
                        try
                        {
                            ConfigurePropertyColumns();
                        }
                        catch
                        {
                            // Will be configured on DataBindingComplete event
                        }

                        _lblPropertiesStatus.Text = $"Total Properties: {properties.Count}";
                        _lblPropertiesStatus.ForeColor = Color.Green;
                    }
                    catch (Exception ex)
                    {
                        _lblPropertiesStatus.Text = $"Error displaying properties: {ex.Message}";
                        _lblPropertiesStatus.ForeColor = Color.Firebrick;
                        System.Diagnostics.Debug.WriteLine($"Error in LoadProperties display: {ex.Message}");
                    }
                }
                else
                {
                    _dgvProperties.DataSource = null;
                    _dgvProperties.Rows.Clear();
                    _lblPropertiesStatus.Text = "No properties found.";
                    _lblPropertiesStatus.ForeColor = Color.Blue;
                }
            }
            catch (SqlException sqlEx)
            {
                if (sqlEx.Number == 208) // Invalid object name
                {
                    _dgvProperties.DataSource = null;
                    _dgvProperties.Rows.Clear();
                    _lblPropertiesStatus.Text = "Properties table not found. Please run the database setup script first.";
                    _lblPropertiesStatus.ForeColor = Color.Firebrick;
                }
                else
                {
                    _lblPropertiesStatus.Text = $"Database error: {sqlEx.Message}";
                    _lblPropertiesStatus.ForeColor = Color.Firebrick;
                }
            }
            catch (Exception ex)
            {
                _lblPropertiesStatus.Text = $"Error loading properties: {ex.Message}";
                _lblPropertiesStatus.ForeColor = Color.Firebrick;
                System.Diagnostics.Debug.WriteLine($"Error in LoadProperties: {ex.Message}");
            }
        }

        private void ConfigureUserColumns()
        {
            try
            {
                if (_dgvUsers == null || _dgvUsers.Columns == null || _dgvUsers.Columns.Count == 0)
                    return;

                if (_dgvUsers.InvokeRequired)
                {
                    _dgvUsers.Invoke(new Action(ConfigureUserColumns));
                    return;
                }

                // Use safer column access
                var userIdCol = _dgvUsers.Columns.Contains("UserId") ? _dgvUsers.Columns["UserId"] : null;
                if (userIdCol != null)
                    userIdCol.Visible = false;

                var usernameCol = _dgvUsers.Columns.Contains("Username") ? _dgvUsers.Columns["Username"] : null;
                if (usernameCol != null)
                {
                    usernameCol.HeaderText = "Username";
                    usernameCol.Width = 200;
                }

                var emailCol = _dgvUsers.Columns.Contains("Email") ? _dgvUsers.Columns["Email"] : null;
                if (emailCol != null)
                {
                    emailCol.HeaderText = "Email";
                    emailCol.Width = 300;
                }

                var userRoleCol = _dgvUsers.Columns.Contains("UserRole") ? _dgvUsers.Columns["UserRole"] : null;
                if (userRoleCol != null)
                {
                    userRoleCol.HeaderText = "Role";
                    userRoleCol.Width = 150;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ConfigureUserColumns: {ex.Message}");
            }
        }

        private void ConfigurePropertyColumns()
        {
            try
            {
                if (_dgvProperties == null || _dgvProperties.Columns == null || _dgvProperties.Columns.Count == 0)
                    return;

                if (_dgvProperties.InvokeRequired)
                {
                    _dgvProperties.Invoke(new Action(ConfigurePropertyColumns));
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
                    sellerNameCol.Width = 150;
                }

                var descriptionCol = _dgvProperties.Columns.Contains("Description") ? _dgvProperties.Columns["Description"] : null;
                if (descriptionCol != null)
                {
                    descriptionCol.HeaderText = "Description";
                    descriptionCol.Width = 200;
                }

                var propertyTypeCol = _dgvProperties.Columns.Contains("PropertyType") ? _dgvProperties.Columns["PropertyType"] : null;
                if (propertyTypeCol != null)
                {
                    propertyTypeCol.HeaderText = "Type";
                    propertyTypeCol.Width = 100;
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
                    priceCol.Width = 120;
                }

                var bedroomsCol = _dgvProperties.Columns.Contains("Bedrooms") ? _dgvProperties.Columns["Bedrooms"] : null;
                if (bedroomsCol != null)
                {
                    bedroomsCol.HeaderText = "Bedrooms";
                    bedroomsCol.Width = 80;
                }

                var bathroomsCol = _dgvProperties.Columns.Contains("Bathrooms") ? _dgvProperties.Columns["Bathrooms"] : null;
                if (bathroomsCol != null)
                {
                    bathroomsCol.HeaderText = "Bathrooms";
                    bathroomsCol.Width = 80;
                }

                var areaCol = _dgvProperties.Columns.Contains("Area") ? _dgvProperties.Columns["Area"] : null;
                if (areaCol != null)
                {
                    areaCol.HeaderText = "Area";
                    areaCol.Width = 100;
                }

                var statusCol = _dgvProperties.Columns.Contains("Status") ? _dgvProperties.Columns["Status"] : null;
                if (statusCol != null)
                {
                    statusCol.HeaderText = "Status";
                    statusCol.Width = 100;
                }

                var createdDateCol = _dgvProperties.Columns.Contains("CreatedDate") ? _dgvProperties.Columns["CreatedDate"] : null;
                if (createdDateCol != null)
                {
                    createdDateCol.HeaderText = "Created Date";
                    createdDateCol.Width = 120;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ConfigurePropertyColumns: {ex.Message}");
            }
        }

        private UserViewModel GetSelectedUser()
        {
            if (_dgvUsers.SelectedRows.Count == 0)
                return null;

            var selectedRow = _dgvUsers.SelectedRows[0];
            if (selectedRow.DataBoundItem == null)
                return null;

            try
            {
                var userId = Convert.ToInt32(selectedRow.Cells["UserId"].Value);
                return _userRepo.GetUserById(userId);
            }
            catch
            {
                return null;
            }
        }

        private PropertyViewModel GetSelectedProperty()
        {
            if (_dgvProperties.SelectedRows.Count == 0)
                return null;

            var selectedRow = _dgvProperties.SelectedRows[0];
            if (selectedRow.DataBoundItem == null)
                return null;

            try
            {
                var propertyId = Convert.ToInt32(selectedRow.Cells["PropertyId"].Value);
                var property = _propertyRepo.GetPropertyById(propertyId);
                if (property == null)
                    return null;

                // Get seller name
                var user = _userRepo.GetUserById(property.SellerId);
                return new PropertyViewModel
                {
                    PropertyId = property.PropertyId,
                    SellerId = property.SellerId,
                    SellerName = user?.Username ?? "Unknown",
                    Title = property.Title,
                    Description = property.Description,
                    PropertyType = property.PropertyType,
                    Address = property.Address,
                    Price = property.Price,
                    Bedrooms = property.Bedrooms,
                    Bathrooms = property.Bathrooms,
                    Area = property.Area,
                    Status = property.Status,
                    CreatedDate = property.CreatedDate,
                    ModifiedDate = property.ModifiedDate
                };
            }
            catch
            {
                return null;
            }
        }

        private void OnAddUserClick(object sender, EventArgs e)
        {
            using (var form = new UserForm())
            {
                if (form.ShowDialog(this) == DialogResult.OK && form.UserSaved)
                {
                    LoadUsers();
                    _lblUsersStatus.Text = "User added successfully!";
                    _lblUsersStatus.ForeColor = Color.Green;
                }
            }
        }

        private void OnEditUserClick(object sender, EventArgs e)
        {
            var user = GetSelectedUser();
            if (user == null)
            {
                MessageBox.Show("Please select a user to edit.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var form = new UserForm(user))
            {
                if (form.ShowDialog(this) == DialogResult.OK && form.UserSaved)
                {
                    LoadUsers();
                    _lblUsersStatus.Text = "User updated successfully!";
                    _lblUsersStatus.ForeColor = Color.Green;
                }
            }
        }

        private void OnDeleteUserClick(object sender, EventArgs e)
        {
            var user = GetSelectedUser();
            if (user == null)
            {
                MessageBox.Show("Please select a user to delete.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to delete user '{user.Username}'?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    if (_userRepo.DeleteUser(user.UserId))
                    {
                        LoadUsers();
                        _lblUsersStatus.Text = "User deleted successfully!";
                        _lblUsersStatus.ForeColor = Color.Green;
                    }
                    else
                    {
                        _lblUsersStatus.Text = "Failed to delete user.";
                        _lblUsersStatus.ForeColor = Color.Firebrick;
                    }
                }
                catch (Exception ex)
                {
                    _lblUsersStatus.Text = $"Error deleting user: {ex.Message}";
                    _lblUsersStatus.ForeColor = Color.Firebrick;
                }
            }
        }

        private void OnAddPropertyClick(object sender, EventArgs e)
        {
            // Get first seller ID for default (admin mode)
            var sellers = _userRepo.GetAllUsers().Where(u => u.UserRole == "Seller").ToList();
            if (sellers.Count == 0)
            {
                MessageBox.Show("No sellers found. Please create a seller account first.", "No Sellers", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var form = new AddPropertyForm(sellers[0].UserId, null, true))
            {
                if (form.ShowDialog(this) == DialogResult.OK && form.PropertyAdded)
                {
                    LoadProperties();
                    _lblPropertiesStatus.Text = "Property added successfully!";
                    _lblPropertiesStatus.ForeColor = Color.Green;
                }
            }
        }

        private void OnEditPropertyClick(object sender, EventArgs e)
        {
            var propertyViewModel = GetSelectedProperty();
            if (propertyViewModel == null)
            {
                MessageBox.Show("Please select a property to edit.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var property = _propertyRepo.GetPropertyById(propertyViewModel.PropertyId);
            if (property == null)
            {
                MessageBox.Show("Property not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (var form = new AddPropertyForm(property.SellerId, property, true))
            {
                if (form.ShowDialog(this) == DialogResult.OK && form.PropertyAdded)
                {
                    LoadProperties();
                    _lblPropertiesStatus.Text = "Property updated successfully!";
                    _lblPropertiesStatus.ForeColor = Color.Green;
                }
            }
        }

        private void OnDeletePropertyClick(object sender, EventArgs e)
        {
            var propertyViewModel = GetSelectedProperty();
            if (propertyViewModel == null)
            {
                MessageBox.Show("Please select a property to delete.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to delete property '{propertyViewModel.Title}'?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    if (_propertyRepo.DeleteProperty(propertyViewModel.PropertyId))
                    {
                        LoadProperties();
                        _lblPropertiesStatus.Text = "Property deleted successfully!";
                        _lblPropertiesStatus.ForeColor = Color.Green;
                    }
                    else
                    {
                        _lblPropertiesStatus.Text = "Failed to delete property.";
                        _lblPropertiesStatus.ForeColor = Color.Firebrick;
                    }
                }
                catch (Exception ex)
                {
                    _lblPropertiesStatus.Text = $"Error deleting property: {ex.Message}";
                    _lblPropertiesStatus.ForeColor = Color.Firebrick;
                }
            }
        }
    }
}

