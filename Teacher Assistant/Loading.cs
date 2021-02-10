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
    public partial class Loading : Form
    {
        public Loading()
        {
            InitializeComponent();
        }

        private void Loading_Load(object sender, EventArgs e)
        {
            new Thread(async () =>
            {

                string information = File.ReadAllText(Schoology.program_path + "information.json");

                Dictionary<string, string> product = JsonConvert.DeserializeObject<Dictionary<string, string>>(information);

                bool status = await Schoology.LogIn(product["username"], product["password"], progressBar1);

                Invoke(new Action(() =>
                {
                    if (!status)
                    {
                        Login form = new Login();
                        form.Show();
                        this.Hide();
                    } 
                    else if (!File.Exists(Schoology.program_path + "schedule.json"))
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

        private void Loading_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Schoology.chrome != null)
            {
                Schoology.chrome.Quit();
            }

            Environment.Exit(Environment.ExitCode);
        }
    }
}
