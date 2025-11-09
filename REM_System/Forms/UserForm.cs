using REM_System.Data;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace REM_System.Forms
{
    public sealed class UserForm : Form
    {
        private readonly TextBox _txtUsername = new TextBox();
        private readonly TextBox _txtEmail = new TextBox();
        private readonly TextBox _txtPassword = new TextBox();
        private readonly ComboBox _cmbUserRole = new ComboBox();
        private readonly CheckBox _chkChangePassword = new CheckBox();
        private readonly Button _btnSave = new Button();
        private readonly Button _btnCancel = new Button();
        private readonly Label _lblStatus = new Label();
        private readonly UserViewModel _userToEdit;
        private readonly UserRepository _userRepo = new UserRepository();

        public bool UserSaved { get; private set; } = false;

        public UserForm(UserViewModel userToEdit = null)
        {
            _userToEdit = userToEdit;
            InitializeForm();
            if (userToEdit != null)
            {
                LoadUserData(userToEdit);
            }
        }

        private void InitializeForm()
        {
            Text = _userToEdit == null ? "Add User" : "Edit User";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(450, 280);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            int yPos = 20;
            int labelWidth = 100;
            int controlX = 120;
            int controlWidth = 300;

            var lblTitle = new Label
            {
                Text = _userToEdit == null ? "Add New User" : "Edit User",
                AutoSize = true,
                Font = new Font(Font, FontStyle.Bold),
                Location = new Point(20, yPos)
            };
            yPos += 35;

            var lblUsername = new Label { Text = "Username *", AutoSize = true, Location = new Point(20, yPos) };
            _txtUsername.Location = new Point(controlX, yPos - 2);
            _txtUsername.Width = controlWidth;
            yPos += 30;

            var lblEmail = new Label { Text = "Email *", AutoSize = true, Location = new Point(20, yPos) };
            _txtEmail.Location = new Point(controlX, yPos - 2);
            _txtEmail.Width = controlWidth;
            yPos += 30;

            var lblUserRole = new Label { Text = "Role *", AutoSize = true, Location = new Point(20, yPos) };
            _cmbUserRole.Location = new Point(controlX, yPos - 2);
            _cmbUserRole.Width = controlWidth;
            _cmbUserRole.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbUserRole.Items.AddRange(new object[] { "Buyer", "Seller", "User" });
            _cmbUserRole.SelectedIndex = 0;
            yPos += 30;

            if (_userToEdit != null)
            {
                _chkChangePassword.Text = "Change Password";
                _chkChangePassword.Location = new Point(controlX, yPos);
                _chkChangePassword.CheckedChanged += (s, e) =>
                {
                    _txtPassword.Enabled = _chkChangePassword.Checked;
                    if (!_chkChangePassword.Checked)
                    {
                        _txtPassword.Text = string.Empty;
                    }
                };
                yPos += 30;
            }

            var lblPassword = new Label { Text = "Password *", AutoSize = true, Location = new Point(20, yPos) };
            _txtPassword.Location = new Point(controlX, yPos - 2);
            _txtPassword.Width = controlWidth;
            _txtPassword.UseSystemPasswordChar = true;
            _txtPassword.Enabled = _userToEdit == null || _chkChangePassword.Checked;
            yPos += 40;

            _btnSave.Text = _userToEdit == null ? "Add User" : "Update User";
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
            Controls.Add(lblUsername);
            Controls.Add(_txtUsername);
            Controls.Add(lblEmail);
            Controls.Add(_txtEmail);
            Controls.Add(lblUserRole);
            Controls.Add(_cmbUserRole);
            if (_userToEdit != null)
            {
                Controls.Add(_chkChangePassword);
            }
            Controls.Add(lblPassword);
            Controls.Add(_txtPassword);
            Controls.Add(_btnSave);
            Controls.Add(_btnCancel);
            Controls.Add(_lblStatus);
        }

        private void LoadUserData(UserViewModel user)
        {
            _txtUsername.Text = user.Username;
            _txtEmail.Text = user.Email;
            _cmbUserRole.SelectedItem = user.UserRole;
            if (_cmbUserRole.SelectedItem == null && _cmbUserRole.Items.Count > 0)
            {
                _cmbUserRole.SelectedIndex = 0;
            }
        }

        private void OnSaveClick(object sender, EventArgs e)
        {
            var username = _txtUsername.Text.Trim();
            var email = _txtEmail.Text.Trim();
            var userRole = Convert.ToString(_cmbUserRole.SelectedItem);
            var password = _txtPassword.Text;

            if (string.IsNullOrWhiteSpace(username))
            {
                _lblStatus.Text = "Please enter a username.";
                return;
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                _lblStatus.Text = "Please enter an email.";
                return;
            }

            if (string.IsNullOrWhiteSpace(userRole))
            {
                _lblStatus.Text = "Please select a role.";
                return;
            }

            if (_userToEdit == null && string.IsNullOrWhiteSpace(password))
            {
                _lblStatus.Text = "Please enter a password.";
                return;
            }

            if (_userToEdit != null && _chkChangePassword.Checked && string.IsNullOrWhiteSpace(password))
            {
                _lblStatus.Text = "Please enter a password.";
                return;
            }

            try
            {
                if (_userToEdit == null)
                {
                    // Check if username already exists
                    if (_userRepo.UsernameExists(username))
                    {
                        _lblStatus.Text = "Username already exists.";
                        return;
                    }

                    // Check if email already exists
                    if (_userRepo.EmailExists(email))
                    {
                        _lblStatus.Text = "Email already exists.";
                        return;
                    }

                    var userId = _userRepo.CreateUser(username, password, email, userRole);
                    if (userId > 0)
                    {
                        UserSaved = true;
                        DialogResult = DialogResult.OK;
                    }
                    else
                    {
                        _lblStatus.Text = "Failed to create user.";
                    }
                }
                else
                {
                    // Check if username already exists (excluding current user)
                    if (username != _userToEdit.Username && _userRepo.UsernameExists(username))
                    {
                        _lblStatus.Text = "Username already exists.";
                        return;
                    }

                    // Check if email already exists (excluding current user)
                    if (email != _userToEdit.Email && _userRepo.EmailExists(email))
                    {
                        _lblStatus.Text = "Email already exists.";
                        return;
                    }

                    string passwordToUpdate = _chkChangePassword.Checked ? password : null;
                    if (_userRepo.UpdateUser(_userToEdit.UserId, username, email, userRole, passwordToUpdate))
                    {
                        UserSaved = true;
                        DialogResult = DialogResult.OK;
                    }
                    else
                    {
                        _lblStatus.Text = "Failed to update user.";
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

