using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace Teacher_Assistant
{
    public partial class Home : Form
    {

        private readonly System.Timers.Timer myTimer = new System.Timers.Timer(500);

        bool in_class = false;

        string current_class = null;

        ArrayList students;

        ArrayList missing_students = new ArrayList();

        ArrayList active_students = new ArrayList();

        ArrayList old_loop = new ArrayList();

        public Home()
        {
            InitializeComponent();
        }

        private void Home_Load(object sender, EventArgs e)
        {
            myTimer.Elapsed += Students_Check;
            myTimer.Elapsed += InformationUpdater;
            myTimer.Start();
        }


        private void Students_Check(object sender, ElapsedEventArgs e)
        {

            if (!in_class)
            {
                return;
            }

            ArrayList active_students = Schoology.GetUserList();

            foreach (string i in students)
            {
                if (!active_students.Contains(i))
                {

                    if (old_loop.Contains(i))
                    {
                        old_loop.Remove(i);
                        string now = DateTime.Now.ToString("hh:mm tt");
                        Invoke(new Action(() =>
                        {
                            textBox2.Text = "[" + now + "] " + i + " left the session." + Environment.NewLine + Environment.NewLine + textBox2.Text;
                        }));

                    }

                    if (!missing_students.Contains(i))
                    {
                        missing_students.Add(i);
                    }

                    continue;
                } else
                {
                    if (!old_loop.Contains(i))
                    {
                        old_loop.Add(i);
                    }
                }

                if (missing_students.Contains(i) && active_students.Contains(i))
                {
                    missing_students.Remove(i);
                    if (!old_loop.Contains(i))
                    {
                        old_loop.Add(i);
                    }
                    string now = DateTime.Now.ToString("hh:mm tt");
                    Invoke(new Action(() =>
                    {
                        textBox2.Text = "[" + now + "] " + i + " joined the session." + Environment.NewLine + Environment.NewLine + textBox2.Text;
                    }));
                    continue;
                }

            }

            if (missing_students.Count > 0)
            {
                string final = "";

                foreach (string i in missing_students)
                {
                    final += i + Environment.NewLine + Environment.NewLine;
                }

                if (textBox1.Text != final)
                {
                    Invoke(new Action(() =>
                    {
                        label5.Text = "Missing Students (" + missing_students.Count + ")";
                        textBox1.Text = final;
                    }));
                }
            }

            if (Schoology.HasEnded())
            {
                Invoke(new Action(() =>
                {
                    textBox2.Text = "";
                    textBox1.Text = "";
                    label5.Text = "Missing Students";
                    label4.Text = "I'm not in a class!";
                }));
                in_class = false;
                button1.Enabled = true;
                missing_students = new ArrayList();
                active_students = new ArrayList();
                students = new ArrayList();
                old_loop = new ArrayList();
                current_class = null;
            }


        }

        private void InformationUpdater(object sender, ElapsedEventArgs e)
        {
            string upcoming = Schoology.NextPeriod();

            string starting_in = Schoology.NextPeriodIn();

            string current_period = Schoology.CurrentPeriod();

            Invoke(new Action(() =>
            {

                if (string.IsNullOrEmpty(current_period))
                {
                    label7.Text = "Current: None";
                    current_class = null;
                }
                else
                {
                    label7.Text = "Current: " + current_period;
                    current_class = current_period;
                }

                if (string.IsNullOrEmpty(upcoming))
                {
                    label2.Text = "Next period: None";
                }
                else
                {
                    label2.Text = "Next period: " + upcoming;
                }

                label3.Text = "Starting in: " + starting_in;
            }));

        }

        private void button1_ClickAsync(object sender, EventArgs e)
        {
            label8.Visible = false;

            if (current_class == null)
            {
                return;
            }

            button1.Enabled = false;

            label4.Text = "Joining " + current_class;

            new Thread(async () =>
            {
                string course_id = Schoology.course_to_id[current_class];

                if (Schoology.JoinConference(course_id))
                {
                    students = await Schoology.GetStudentsAsync(current_class);
                    Invoke(new Action(() =>
                    {
                        button1.Enabled = false;
                        label4.Text = "I'm in " + current_class;
                    }));

                    in_class = true;
                }
                else
                {
                    Invoke(new Action(() =>
                    {
                        label4.Text = "I'm not in a class!";
                        label8.Visible = true;
                    }));
                }
            }).Start();
        }

        private void Home_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Schoology.chrome != null)
            {
                Schoology.chrome.Quit();
            }

            Environment.Exit(Environment.ExitCode);
        }

    }
}
