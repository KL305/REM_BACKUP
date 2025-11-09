using REM_System.Data;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace REM_System.Forms
{
    public sealed class AddPropertyForm : Form
    {
        private readonly TextBox _txtTitle = new TextBox();
        private readonly TextBox _txtDescription = new TextBox();
        private readonly ComboBox _cmbPropertyType = new ComboBox();
        private readonly TextBox _txtAddress = new TextBox();
        private readonly NumericUpDown _numPrice = new NumericUpDown();
        private readonly NumericUpDown _numBedrooms = new NumericUpDown();
        private readonly NumericUpDown _numBathrooms = new NumericUpDown();
        private readonly NumericUpDown _numArea = new NumericUpDown();
        private readonly ComboBox _cmbStatus = new ComboBox();
        private readonly Button _btnSave = new Button();
        private readonly Button _btnCancel = new Button();
        private readonly Label _lblStatus = new Label();
        private readonly int _sellerId;
        private readonly bool _isAdminMode;
        private readonly ComboBox _cmbSeller = new ComboBox();
        private Property _propertyToEdit;

        public bool PropertyAdded { get; private set; } = false;

        public AddPropertyForm(int sellerId, Property propertyToEdit = null, bool isAdminMode = false)
        {
            _sellerId = sellerId;
            _isAdminMode = isAdminMode;
            _propertyToEdit = propertyToEdit;
            InitializeForm();
            if (propertyToEdit != null)
            {
                LoadPropertyData(propertyToEdit);
            }
        }

        private void InitializeForm()
        {
            Text = _propertyToEdit == null ? "Add Property" : "Edit Property";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(500, _isAdminMode ? 560 : 520);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            int yPos = 20;
            int labelWidth = 120;
            int controlX = 140;
            int controlWidth = 330;

            var lblTitle = new Label { Text = _propertyToEdit == null ? "Add New Property" : "Edit Property", 
                AutoSize = true, Font = new Font(Font, FontStyle.Bold), Location = new Point(20, yPos) };
            yPos += 35;

            if (_isAdminMode)
            {
                var lblSeller = new Label { Text = "Seller *", AutoSize = true, Location = new Point(20, yPos) };
                _cmbSeller.Location = new Point(controlX, yPos - 2);
                _cmbSeller.Width = controlWidth;
                _cmbSeller.DropDownStyle = ComboBoxStyle.DropDownList;
                LoadSellers();
                Controls.Add(lblSeller);
                Controls.Add(_cmbSeller);
                yPos += 30;
            }

            var lblPropertyTitle = new Label { Text = "Title *", AutoSize = true, Location = new Point(20, yPos) };
            _txtTitle.Location = new Point(controlX, yPos - 2);
            _txtTitle.Width = controlWidth;
            yPos += 30;

            var lblPropertyType = new Label { Text = "Type *", AutoSize = true, Location = new Point(20, yPos) };
            _cmbPropertyType.Location = new Point(controlX, yPos - 2);
            _cmbPropertyType.Width = controlWidth;
            _cmbPropertyType.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbPropertyType.Items.AddRange(new object[] { "RealEstate", "Product" });
            _cmbPropertyType.SelectedIndex = 0;
            yPos += 30;

            var lblDescription = new Label { Text = "Description", AutoSize = true, Location = new Point(20, yPos) };
            _txtDescription.Location = new Point(controlX, yPos - 2);
            _txtDescription.Width = controlWidth;
            _txtDescription.Height = 60;
            _txtDescription.Multiline = true;
            _txtDescription.ScrollBars = ScrollBars.Vertical;
            yPos += 70;

            var lblAddress = new Label { Text = "Address", AutoSize = true, Location = new Point(20, yPos) };
            _txtAddress.Location = new Point(controlX, yPos - 2);
            _txtAddress.Width = controlWidth;
            yPos += 30;

            var lblPrice = new Label { Text = "Price *", AutoSize = true, Location = new Point(20, yPos) };
            _numPrice.Location = new Point(controlX, yPos - 2);
            _numPrice.Width = controlWidth;
            _numPrice.DecimalPlaces = 2;
            _numPrice.Minimum = 0;
            _numPrice.Maximum = 999999999;
            yPos += 30;

            var lblBedrooms = new Label { Text = "Bedrooms", AutoSize = true, Location = new Point(20, yPos) };
            _numBedrooms.Location = new Point(controlX, yPos - 2);
            _numBedrooms.Width = controlWidth;
            _numBedrooms.Minimum = 0;
            _numBedrooms.Maximum = 50;
            _numBedrooms.Value = 0;
            yPos += 30;

            var lblBathrooms = new Label { Text = "Bathrooms", AutoSize = true, Location = new Point(20, yPos) };
            _numBathrooms.Location = new Point(controlX, yPos - 2);
            _numBathrooms.Width = controlWidth;
            _numBathrooms.Minimum = 0;
            _numBathrooms.Maximum = 50;
            _numBathrooms.Value = 0;
            yPos += 30;

            var lblArea = new Label { Text = "Area (sq ft)", AutoSize = true, Location = new Point(20, yPos) };
            _numArea.Location = new Point(controlX, yPos - 2);
            _numArea.Width = controlWidth;
            _numArea.DecimalPlaces = 2;
            _numArea.Minimum = 0;
            _numArea.Maximum = 999999999;
            _numArea.Value = 0;
            yPos += 30;

            var lblStatus = new Label { Text = "Status *", AutoSize = true, Location = new Point(20, yPos) };
            _cmbStatus.Location = new Point(controlX, yPos - 2);
            _cmbStatus.Width = controlWidth;
            _cmbStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbStatus.Items.AddRange(new object[] { "Available", "Sold", "Pending" });
            _cmbStatus.SelectedIndex = 0;
            yPos += 40;

            _btnSave.Text = _propertyToEdit == null ? "Add Property" : "Update Property";
            _btnSave.Location = new Point(controlX, yPos);
            _btnSave.Width = 120;
            _btnSave.Click += OnSaveClick;

            _btnCancel.Text = "Cancel";
            _btnCancel.Location = new Point(controlX + 130, yPos);
            _btnCancel.Width = 120;
            _btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

            _lblStatus.AutoSize = true;
            _lblStatus.ForeColor = Color.Firebrick;
            _lblStatus.Location = new Point(20, yPos + 35);

            Controls.Add(lblTitle);
            Controls.Add(lblPropertyTitle);
            Controls.Add(_txtTitle);
            Controls.Add(lblPropertyType);
            Controls.Add(_cmbPropertyType);
            Controls.Add(lblDescription);
            Controls.Add(_txtDescription);
            Controls.Add(lblAddress);
            Controls.Add(_txtAddress);
            Controls.Add(lblPrice);
            Controls.Add(_numPrice);
            Controls.Add(lblBedrooms);
            Controls.Add(_numBedrooms);
            Controls.Add(lblBathrooms);
            Controls.Add(_numBathrooms);
            Controls.Add(lblArea);
            Controls.Add(_numArea);
            Controls.Add(lblStatus);
            Controls.Add(_cmbStatus);
            Controls.Add(_btnSave);
            Controls.Add(_btnCancel);
            Controls.Add(_lblStatus);
        }

        private void LoadSellers()
        {
            try
            {
                var userRepo = new UserRepository();
                var allUsers = userRepo.GetAllUsers();
                var sellers = allUsers.Where(u => u.UserRole == "Seller").ToList();
                
                _cmbSeller.Items.Clear();
                foreach (var seller in sellers)
                {
                    _cmbSeller.Items.Add(new { SellerId = seller.UserId, SellerName = seller.Username });
                }
                
                if (_cmbSeller.Items.Count > 0)
                {
                    _cmbSeller.DisplayMember = "SellerName";
                    _cmbSeller.ValueMember = "SellerId";
                    _cmbSeller.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading sellers: {ex.Message}");
            }
        }

        private void LoadPropertyData(Property property)
        {
            _txtTitle.Text = property.Title;
            _cmbPropertyType.SelectedItem = property.PropertyType;
            _txtDescription.Text = property.Description;
            _txtAddress.Text = property.Address;
            _numPrice.Value = property.Price;
            _numBedrooms.Value = property.Bedrooms ?? 0;
            _numBathrooms.Value = property.Bathrooms ?? 0;
            _numArea.Value = property.Area ?? 0;
            _cmbStatus.SelectedItem = property.Status;
            
            if (_isAdminMode && _cmbSeller.Items.Count > 0)
            {
                // Find and select the seller
                for (int i = 0; i < _cmbSeller.Items.Count; i++)
                {
                    dynamic item = _cmbSeller.Items[i];
                    if (item.SellerId == property.SellerId)
                    {
                        _cmbSeller.SelectedIndex = i;
                        break;
                    }
                }
            }
        }

        private void OnSaveClick(object sender, EventArgs e)
        {
            var title = _txtTitle.Text.Trim();
            var propertyType = Convert.ToString(_cmbPropertyType.SelectedItem);
            var price = _numPrice.Value;

            if (string.IsNullOrWhiteSpace(title))
            {
                _lblStatus.Text = "Please enter a title.";
                return;
            }

            if (price <= 0)
            {
                _lblStatus.Text = "Please enter a valid price.";
                return;
            }

            int selectedSellerId = _sellerId;
            if (_isAdminMode)
            {
                if (_cmbSeller.SelectedItem == null)
                {
                    _lblStatus.Text = "Please select a seller.";
                    return;
                }
                dynamic selectedSeller = _cmbSeller.SelectedItem;
                selectedSellerId = selectedSeller.SellerId;
            }

            try
            {
                var repo = new PropertyRepository();
                Property property;

                if (_propertyToEdit != null)
                {
                    property = _propertyToEdit;
                    property.SellerId = selectedSellerId;
                    property.Title = title;
                    property.Description = _txtDescription.Text.Trim();
                    property.PropertyType = propertyType;
                    property.Address = _txtAddress.Text.Trim();
                    property.Price = price;
                    property.Bedrooms = _numBedrooms.Value > 0 ? (int?)_numBedrooms.Value : null;
                    property.Bathrooms = _numBathrooms.Value > 0 ? (int?)_numBathrooms.Value : null;
                    property.Area = _numArea.Value > 0 ? (decimal?)_numArea.Value : null;
                    property.Status = Convert.ToString(_cmbStatus.SelectedItem);

                    bool success;
                    if (_isAdminMode)
                    {
                        success = repo.UpdatePropertyAdmin(property);
                    }
                    else
                    {
                        success = repo.UpdateProperty(property);
                    }

                    if (success)
                    {
                        PropertyAdded = true;
                        DialogResult = DialogResult.OK;
                    }
                    else
                    {
                        _lblStatus.Text = "Failed to update property.";
                    }
                }
                else
                {
                    property = new Property
                    {
                        SellerId = selectedSellerId,
                        Title = title,
                        Description = _txtDescription.Text.Trim(),
                        PropertyType = propertyType,
                        Address = _txtAddress.Text.Trim(),
                        Price = price,
                        Bedrooms = _numBedrooms.Value > 0 ? (int?)_numBedrooms.Value : null,
                        Bathrooms = _numBathrooms.Value > 0 ? (int?)_numBathrooms.Value : null,
                        Area = _numArea.Value > 0 ? (decimal?)_numArea.Value : null,
                        Status = Convert.ToString(_cmbStatus.SelectedItem)
                    };

                    var propertyId = repo.CreateProperty(property);
                    if (propertyId > 0)
                    {
                        PropertyAdded = true;
                        DialogResult = DialogResult.OK;
                    }
                    else
                    {
                        _lblStatus.Text = "Failed to add property.";
                    }
                }
            }
            catch (Exception ex)
            {
                _lblStatus.Text = $"Error: {ex.Message}";
            }
        }
    }
}

