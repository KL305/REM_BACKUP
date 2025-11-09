using REM_System.Data;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace REM_System.Forms
{
    public sealed class LoginForm : Form
    {
        private readonly TextBox _txtUsername = new TextBox();
        private readonly TextBox _txtPassword = new TextBox();
        private readonly Button _btnLogin = new Button();
        private readonly Button _btnRegister = new Button();
        private readonly Label _lblStatus = new Label();

        public LoginForm()
        {
            Text = "Login";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(380, 240);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            var lblTitle = new Label { Text = "RealEstate - Login", AutoSize = true, Font = new Font(Font, FontStyle.Bold), Location = new Point(20, 20) };
            var lblUsername = new Label { Text = "Username", AutoSize = true, Location = new Point(20, 60) };
            var lblPassword = new Label { Text = "Password", AutoSize = true, Location = new Point(20, 100) };

            _txtUsername.Location = new Point(110, 56);
            _txtUsername.Width = 230;
            _txtPassword.Location = new Point(110, 96);
            _txtPassword.Width = 230;
            _txtPassword.UseSystemPasswordChar = true;

            _btnLogin.Text = "Login";
            _btnLogin.Location = new Point(110, 140);
            _btnLogin.Width = 100;
            _btnLogin.Click += OnLoginClick;

            _btnRegister.Text = "Register";
            _btnRegister.Location = new Point(240, 140);
            _btnRegister.Width = 100;
            _btnRegister.Click += OnRegisterClick;

            _lblStatus.ForeColor = Color.Firebrick;
            _lblStatus.AutoSize = true;
            _lblStatus.Location = new Point(20, 180);

            Controls.Add(lblTitle);
            Controls.Add(lblUsername);
            Controls.Add(lblPassword);
            Controls.Add(_txtUsername);
            Controls.Add(_txtPassword);
            Controls.Add(_btnLogin);
            Controls.Add(_btnRegister);
            Controls.Add(_lblStatus);
        }

        private void OnRegisterClick(object sender, EventArgs e)
        {
            using (var reg = new RegisterForm())
            {
                if (reg.ShowDialog(this) == DialogResult.OK)
                {
                    _txtUsername.Text = reg.RegisteredUsername;
                    _lblStatus.ForeColor = Color.Green;
                    _lblStatus.Text = "Registration successful. You can now login.";
                }
            }
        }

        private void OnLoginClick(object sender, EventArgs e)
        {
            var username = _txtUsername.Text.Trim();
            var password = _txtPassword.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                _lblStatus.ForeColor = Color.Firebrick;
                _lblStatus.Text = "Please enter username and password.";
                return;
            }

            try
            {
                var repo = new UserRepository();
                if (repo.ValidateCredentials(username, password, out var role, out var userId))
                {
                    Hide();
                    
                    if (role == "Admin")
                    {
                        using (var adminDashboard = new AdminDashboardForm(username))
                        {
                            adminDashboard.ShowDialog(this);
                        }
                    }
                    else if (role == "Seller")
                    {
                        using (var sellerDashboard = new SellerDashboardForm(userId, username))
                        {
                            sellerDashboard.ShowDialog(this);
                        }
                    }
                    else if (role == "Buyer")
                    {
                        using (var buyerDashboard = new BuyerDashboardForm(username))
                        {
                            buyerDashboard.ShowDialog(this);
                        }
                    }
                    else
                    {
                        using (var main = new Form1())
                        {
                            main.Text = $"REM System - {username} ({role})";
                            main.StartPosition = FormStartPosition.CenterScreen;
                            main.ShowDialog(this);
                        }
                    }
                    Show();
                }
                else
                {
                    _lblStatus.ForeColor = Color.Firebrick;
                    _lblStatus.Text = "Invalid username or password.";
                }
            }
            catch (Exception ex)
            {
                _lblStatus.ForeColor = Color.Firebrick;
                _lblStatus.Text = $"Error: {ex.Message}";
            }
        }
    }
}


