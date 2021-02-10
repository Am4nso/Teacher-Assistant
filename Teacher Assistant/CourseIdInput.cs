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
    public partial class CourseIdInput : Form
    {

        ArrayList classes = new ArrayList();

        string current_class;
        int current_class_index;
        int max;

        public CourseIdInput()
        {

            foreach (Dictionary<string, string> i in Schoology.schedule.Values)
            {
                foreach (string j in i.Values)
                {
                    if (!classes.Contains(j))
                    {
                        classes.Add(j);
                    }
                }
            }

            InitializeComponent();

            label2.Text = "Course ID for " + classes[0];
            current_class = (string)classes[0];
            current_class_index = 1;
            max = classes.Count;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            textBox1.Enabled = false;

            Schoology.course_to_id.Add(current_class, textBox1.Text);

            if (max == current_class_index)
            {
                File.WriteAllText(Schoology.program_path + "courses.json", JsonConvert.SerializeObject(Schoology.course_to_id));

                Home form = new Home();
                form.Show();
                this.Hide();
                return;
            }

            label2.Text = "Course ID for " + classes[current_class_index];
            current_class = (string)classes[current_class_index];
            current_class_index++;

            textBox1.Clear();
            button1.Enabled = true;
            textBox1.Enabled = true;
        }

        private void CourseIdInput_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Schoology.chrome != null)
            {
                Schoology.chrome.Quit();
            }

            Environment.Exit(Environment.ExitCode);
        }

        
    }
}
