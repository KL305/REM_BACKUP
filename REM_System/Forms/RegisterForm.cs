using REM_System.Data;
using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace REM_System.Forms
{
    public sealed class RegisterForm : Form
    {
        private readonly TextBox _txtUsername = new TextBox();
        private readonly TextBox _txtEmail = new TextBox();
        private readonly TextBox _txtPassword = new TextBox();
        private readonly TextBox _txtConfirm = new TextBox();
        private readonly ComboBox _cmbRole = new ComboBox();
        private readonly Button _btnSubmit = new Button();
        private readonly Button _btnCancel = new Button();
        private readonly Label _lblStatus = new Label();

        public string RegisteredUsername { get; private set; } = string.Empty;

        public RegisterForm()
        {
            Text = "Register";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(420, 320);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            var lblTitle = new Label { Text = "Create Account", AutoSize = true, Font = new Font(Font, FontStyle.Bold), Location = new Point(20, 20) };
            var lblUsername = new Label { Text = "Username", AutoSize = true, Location = new Point(20, 60) };
            var lblEmail = new Label { Text = "Email", AutoSize = true, Location = new Point(20, 95) };
            var lblPassword = new Label { Text = "Password", AutoSize = true, Location = new Point(20, 130) };
            var lblConfirm = new Label { Text = "Confirm", AutoSize = true, Location = new Point(20, 165) };
            var lblRole = new Label { Text = "Role", AutoSize = true, Location = new Point(20, 200) };

            _txtUsername.Location = new Point(110, 56);
            _txtUsername.Width = 280;
            _txtEmail.Location = new Point(110, 91);
            _txtEmail.Width = 280;
            _txtPassword.Location = new Point(110, 126);
            _txtPassword.Width = 280;
            _txtPassword.UseSystemPasswordChar = true;
            _txtConfirm.Location = new Point(110, 161);
            _txtConfirm.Width = 280;
            _txtConfirm.UseSystemPasswordChar = true;

            _cmbRole.Location = new Point(110, 196);
            _cmbRole.Width = 280;
            _cmbRole.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbRole.Items.AddRange(new object[] { "User", "Buyer", "Seller", "Admin" });
            _cmbRole.SelectedIndex = 1; // Default to "Buyer"

            _btnSubmit.Text = "Register";
            _btnSubmit.Location = new Point(190, 240);
            _btnSubmit.Width = 90;
            _btnSubmit.Click += OnSubmitClick;

            _btnCancel.Text = "Cancel";
            _btnCancel.Location = new Point(300, 240);
            _btnCancel.Width = 90;
            _btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

            _lblStatus.AutoSize = true;
            _lblStatus.ForeColor = Color.Firebrick;
            _lblStatus.Location = new Point(20, 280);

            Controls.Add(lblTitle);
            Controls.Add(lblUsername);
            Controls.Add(lblEmail);
            Controls.Add(lblPassword);
            Controls.Add(lblConfirm);
            Controls.Add(lblRole);
            Controls.Add(_txtUsername);
            Controls.Add(_txtEmail);
            Controls.Add(_txtPassword);
            Controls.Add(_txtConfirm);
            Controls.Add(_cmbRole);
            Controls.Add(_btnSubmit);
            Controls.Add(_btnCancel);
            Controls.Add(_lblStatus);
        }

        private void OnSubmitClick(object sender, EventArgs e)
        {
            var username = _txtUsername.Text.Trim();
            var email = _txtEmail.Text.Trim();
            var password = _txtPassword.Text;
            var confirm = _txtConfirm.Text;
            var role = Convert.ToString(_cmbRole.SelectedItem) ?? "User";

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                _lblStatus.Text = "Please fill in all required fields.";
                return;
            }

            if (!IsValidEmail(email))
            {
                _lblStatus.Text = "Please enter a valid email address.";
                return;
            }

            if (password.Length < 6)
            {
                _lblStatus.Text = "Password must be at least 6 characters.";
                return;
            }

            if (!string.Equals(password, confirm, StringComparison.Ordinal))
            {
                _lblStatus.Text = "Passwords do not match.";
                return;
            }

            try
            {
                var repo = new UserRepository();
                if (repo.UsernameExists(username))
                {
                    _lblStatus.Text = "Username already exists.";
                    return;
                }

                if (repo.EmailExists(email))
                {
                    _lblStatus.Text = "Email already exists.";
                    return;
                }

                var userId = repo.CreateUser(username, password, email, role);
                if (userId > 0)
                {
                    RegisteredUsername = username;
                    DialogResult = DialogResult.OK;
                }
                else
                {
                    _lblStatus.Text = "Registration failed.";
                }
            }
            catch (SqlException ex)
            {
                _lblStatus.Text = $"Database error: {ex.Message}";
            }
            catch (Exception ex)
            {
                _lblStatus.Text = $"Error: {ex.Message}";
            }
        }

        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            try
            {
                return Regex.IsMatch(email,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    RegexOptions.IgnoreCase);
            }
            catch
            {
                return false;
            }
        }
    }
}


