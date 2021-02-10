using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Teacher_Assistant
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            textBox1.Enabled = false;
            textBox2.Enabled = false;
            label5.Visible = false;

            string username = textBox1.Text;
            string password = textBox2.Text;

            new Thread(async () =>
            {
                bool result = await Schoology.LogIn(username, password);

                if (!result)
                {
                    Invoke(new Action(() =>
                    {
                        button1.Enabled = true;
                        textBox1.Enabled = true;
                        textBox2.Enabled = true;
                        label5.Visible = true;
                    }));
                    return;
                }

                File.WriteAllText(Schoology.program_path + "information.json", "{\"username\": \"" + username + "\", \"password\": \"" + password + "\"}");

                Invoke(new Action(() =>
                {
                    if (!File.Exists(Schoology.program_path + "schedule.json"))
                    {
                        ScheduleCreator form = new ScheduleCreator();
                        form.Show();
                        this.Hide();
                    }
                    else if (!File.Exists(Schoology.program_path + "courses.json"))
                    {
                        CourseIdInput form = new CourseIdInput();
                        form.Show();
                        this.Hide();
                    }
                    else
                    {
                        Home form = new Home();
                        form.Show();
                        this.Hide();
                    }

                }));

            }).Start();
        }

        private void Login_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Schoology.chrome != null)
            {
                Schoology.chrome.Quit();
            }

            Environment.Exit(Environment.ExitCode);
        }
    }
}
