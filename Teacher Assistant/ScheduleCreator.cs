using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Teacher_Assistant
{
    public partial class ScheduleCreator : Form
    {

        public static ArrayList sunday = new ArrayList();
        public static ArrayList monday = new ArrayList();
        public static ArrayList tuesday = new ArrayList();
        public static ArrayList wednesday = new ArrayList();
        public static ArrayList thursday = new ArrayList();


        public ScheduleCreator()
        {
            InitializeComponent();

            sunday.Add(comboBox1);
            sunday.Add(comboBox2);
            sunday.Add(comboBox3);
            sunday.Add(comboBox4);
            sunday.Add(comboBox5);
            sunday.Add(comboBox6);

            monday.Add(comboBox12);
            monday.Add(comboBox11);
            monday.Add(comboBox10);
            monday.Add(comboBox9);
            monday.Add(comboBox8);
            monday.Add(comboBox7);

            tuesday.Add(comboBox18);
            tuesday.Add(comboBox17);
            tuesday.Add(comboBox16);
            tuesday.Add(comboBox15);
            tuesday.Add(comboBox14);
            tuesday.Add(comboBox13);

            wednesday.Add(comboBox24);
            wednesday.Add(comboBox23);
            wednesday.Add(comboBox22);
            wednesday.Add(comboBox21);
            wednesday.Add(comboBox20);
            wednesday.Add(comboBox19);

            thursday.Add(comboBox30);
            thursday.Add(comboBox29);
            thursday.Add(comboBox28);
            thursday.Add(comboBox27);
            thursday.Add(comboBox26);
            thursday.Add(comboBox25);
        }

        private void Done_Click(object sender, EventArgs e)
        {

            button1.Enabled = false;
            
            int i = 1;

            foreach (ComboBox box in sunday)
            {
                if (string.IsNullOrEmpty(box.Text))
                {
                    i++;
                    continue;
                }

                string time = GetTimeFromPeriod(i);

                Schoology.schedule["Sunday"].Add(time, box.Text);

                i++;
            }

            i = 1;

            foreach (ComboBox box in monday)
            {
                if (string.IsNullOrEmpty(box.Text))
                {
                    i++;
                    continue;
                }

                string time = GetTimeFromPeriod(i);

                Schoology.schedule["Monday"].Add(time, box.Text);

                i++;
            }

            i = 1;

            foreach (ComboBox box in tuesday)
            {
                if (string.IsNullOrEmpty(box.Text))
                {
                    i++;
                    continue;
                }

                string time = GetTimeFromPeriod(i);

                Schoology.schedule["Tuesday"].Add(time, box.Text);

                i++;
            }

            i = 1;

            foreach (ComboBox box in wednesday)
            {
                if (string.IsNullOrEmpty(box.Text))
                {
                    i++;
                    continue;
                }

                string time = GetTimeFromPeriod(i);

                Schoology.schedule["Wednesday"].Add(time, box.Text);

                i++;
            }

            i = 1;

            foreach (ComboBox box in thursday)
            {
                if (string.IsNullOrEmpty(box.Text))
                {
                    i++;
                    continue;
                }

                string time = GetTimeFromPeriod(i);

                Schoology.schedule["Thursday"].Add(time, box.Text);

                i++;
            }

            File.WriteAllText(Schoology.program_path + "schedule.json", JsonConvert.SerializeObject(Schoology.schedule));

            CourseIdInput form = new CourseIdInput();

            form.Show();
            this.Hide();

        }
        
        string GetTimeFromPeriod(int integer)
        {

            string time = "";

            switch (integer)
            {

                case 1:
                    time = "8:00";
                    break;

                case 2:
                    time = "8:55";
                    break;

                case 3:
                    time = "10:05";
                    break;

                case 4:
                    time = "11:00";
                    break;

                case 5:
                    time = "13:00";
                    break;

                case 6:
                    time = "13:55";
                    break;
            }

            return time;
        }

        private void ScheduleCreator_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Schoology.chrome != null)
            {
                Schoology.chrome.Quit();
            }

            Environment.Exit(Environment.ExitCode);
        }
    }
}
